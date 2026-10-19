using UnityEngine;
using UnityEngine.AI;

namespace Arcatech.Units.Combat
{
    /// <summary>
    /// Участник боя на враге: шкала агрессии и слот, назначенный координатором.
    /// Добавляется автоматически из NPCBehaviorWrapper, можно положить в префаб вручную.
    /// Параметры агрессии берутся из EnemyData_SO врага.
    /// </summary>
    [DisallowMultipleComponent]
    public class CombatParticipant : MonoBehaviour
    {
        public const float MaxAggression = 1.5f;
        private const float SlotRequestMemory = 0.5f;
        private const float DamagePenaltyCooldown = 0.2f;
        private const float SummonCombatDelay = 0.1f;

        [SerializeField, Tooltip("Текущая агрессия (только для просмотра).")]
        private float aggression;

        private NPCBehaviorWrapper _wrapper;
        private BaseGameEntityComponent _entity;
        private NavMeshAgent _agent;
        private CombatCoordinator _coordinator;

        private bool _rolledInitial;
        private float _lastDamagePenalty = -10f;
        private float _lastSlotRequest = -10f;
        private float _forceCombatAt = -1f;

        public CombatCoordinator Coordinator => _coordinator;
        public float Aggression => aggression;
        public NavMeshAgent Agent => _agent;
        public NPCBehaviorWrapper Wrapper => _wrapper;

        public bool IsAlive => _entity == null || _entity.EntityAlive;
        public bool IsStunned => _entity != null && _entity.Stunned;
        public bool CanReceiveToken => isActiveAndEnabled && IsAlive && !IsStunned;
        public bool CanMoveToSlot => CanReceiveToken && _agent != null && _agent.enabled && _agent.isOnNavMesh;

        public UnitTier Tier => _wrapper != null ? _wrapper.GetTierInfo : UnitTier.Unassigned;
        public float AttackRange => Profile != null ? Profile.MeleeRange : 2f;

        /// <summary>Нода Move To Engagement Slot недавно просила слот.</summary>
        public bool WantsSlot => Time.time - _lastSlotRequest < SlotRequestMemory;

        public bool HasSlot { get; private set; }
        public Vector3 SlotPosition { get; private set; }
        internal int SlotRing { get; private set; } = -1;
        internal int SlotIndex { get; private set; } = -1;

        private EnemyData_SO Profile => _wrapper != null ? _wrapper.Config : null;

        /// <summary>Участник на объекте; создаётся, если его нет.</summary>
        public static CombatParticipant Of(GameObject go)
        {
            if (go == null) return null;
            if (!go.TryGetComponent(out CombatParticipant p)) p = go.AddComponent<CombatParticipant>();
            return p;
        }

        private void Awake()
        {
            TryGetComponent(out _wrapper);
            TryGetComponent(out _entity);
            TryGetComponent(out _agent);
        }

        private void OnEnable()
        {
            if (_entity != null) _entity.AnnounceDead.AddListener(OnAliveChanged);
            if (_wrapper != null) _wrapper.CombatStateChanged += OnCombatStateChanged;
        }

        private void OnDisable()
        {
            if (_entity != null) _entity.AnnounceDead.RemoveListener(OnAliveChanged);
            if (_wrapper != null) _wrapper.CombatStateChanged -= OnCombatStateChanged;
            Disengage(false);
        }

        #region координатор

        public void Engage(CombatCoordinator coordinator)
        {
            if (coordinator == null || coordinator == _coordinator) return;
            Disengage(false);
            _coordinator = coordinator;

            if (!_rolledInitial)
            {
                _rolledInitial = true;
                var p = Profile;
                float initial = p != null
                    ? Random.Range(p.AggressionOnEngage.x, p.AggressionOnEngage.y)
                    : Random.Range(0.3f, 0.8f);
                aggression = Mathf.Max(aggression, initial); // призванным не занижаем
            }

            coordinator.Register(this);
        }

        public void Disengage(bool died)
        {
            if (_coordinator != null) _coordinator.Unregister(this, died);
            _coordinator = null;
            ClearSlot();
        }

        /// <summary>Координатор выключился сам.</summary>
        internal void ForgetCoordinator(CombatCoordinator coordinator)
        {
            if (_coordinator != coordinator) return;
            _coordinator = null;
            ClearSlot();
        }

        public bool HasToken(AttackTokenCategory category) =>
            _coordinator != null && _coordinator.HasToken(this, category);

        public bool HasAnyToken => _coordinator != null && _coordinator.HasAnyToken(this);

        /// <summary>Держит токен и уже атакует: стоит на месте до Release.</summary>
        public bool IsCommitted => _coordinator != null && _coordinator.IsCommitted(this);

        #endregion

        #region слот

        internal void NotifySlotRequested() => _lastSlotRequest = Time.time;

        internal void SetSlot(int ring, int index, Vector3 position)
        {
            HasSlot = true;
            SlotRing = ring;
            SlotIndex = index;
            SlotPosition = position;
        }

        internal void ClearSlot()
        {
            HasSlot = false;
            SlotRing = -1;
            SlotIndex = -1;
        }

        #endregion

        #region агрессия

        private void Update()
        {
            if (_forceCombatAt > 0f && Time.time >= _forceCombatAt)
            {
                _forceCombatAt = -1f;
                if (_wrapper != null && !_wrapper.InCombat) _wrapper.ForceEnterCombat();
            }

            if (_coordinator == null || IsStunned || HasAnyToken) return;

            var p = Profile;
            float gain = p != null ? p.AggressionGainPerSec : 0.3f;
            AddAggression(gain * Time.deltaTime);
        }

        public void SetAggression(float value) => aggression = Mathf.Clamp(value, 0f, MaxAggression);
        public void AddAggression(float delta) => SetAggression(aggression + delta);

        /// <summary>Атака завершена, токен возвращён.</summary>
        internal void OnAttackFinished()
        {
            var p = Profile;
            SetAggression(p != null ? p.AggressionAfterAttack : 0f);
        }

        internal void OnAllyDied()
        {
            var p = Profile;
            AddAggression(p != null ? p.AggressionOnAllyDeath : 0.25f);
        }

        /// <summary>Вызывается wrapper-ом, когда игрок нанёс урон.</summary>
        public void OnDamagedByPlayer()
        {
            // один удар меняет несколько статов (здоровье, Сила) — штраф раз в DamagePenaltyCooldown
            if (Time.time - _lastDamagePenalty < DamagePenaltyCooldown) return;
            _lastDamagePenalty = Time.time;
            var p = Profile;
            AddAggression(p != null ? p.AggressionOnDamaged : -0.2f);
        }

        /// <summary>
        /// Юнит призван в бою: сразу высокая агрессия, боевая группа призывателя и вход в бой.
        /// </summary>
        public void OnSummoned(float spawnAggression, BaseGameEntityComponent summoner)
        {
            SetAggression(spawnAggression);
            _rolledInitial = true;

            if (_wrapper == null) return;
            if (summoner != null && summoner.TryGetComponent(out NPCBehaviorWrapper summonerWrapper))
                _wrapper.SetCombatGroup(summonerWrapper.CombatGroup);

            // граф стартует не в кадре Instantiate: даём ему подписаться на событие боя
            _forceCombatAt = Time.time + SummonCombatDelay;
        }

        #endregion

        private void OnCombatStateChanged(bool inCombat)
        {
            if (inCombat) return;
            Disengage(false);
            _rolledInitial = false;
            aggression = 0f;
        }

        private void OnAliveChanged(BaseGameEntityComponent entity)
        {
            if (entity != null && !entity.EntityAlive) Disengage(true);
        }
    }
}
