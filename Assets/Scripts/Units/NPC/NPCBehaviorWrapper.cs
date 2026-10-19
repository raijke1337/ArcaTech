using System;
using Arcatech.SaveSystem;
using Arcatech.Stats;
using Arcatech.Units.Combat;
using Arcatech.Units.Control;
using Arcatech.Usables.Effects;
using KBCore.Refs;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

namespace Arcatech.Units
{
    [RequireComponent(typeof(BehaviorGraphAgent), typeof(NavMeshAgent), typeof(UnitInputsComponent))]
    [RequireComponent(typeof(BaseGameEntityComponent), typeof(EntityStatsComponent))]
    [RequireComponent(typeof(EffectsReceiverComponent))]
    public class NPCBehaviorWrapper : ValidatedMonoBehaviour, IKillableComponent, 
        IPausableComponent, IMove,
        ITierProvider,
        IStatUpdatesViewer
    {
        [SerializeField, Self] protected NavMeshAgent agent;
        [SerializeField, Self] protected BehaviorGraphAgent behavior;
        [SerializeField, Self] protected BaseGameEntityComponent entity;
        [SerializeField, Self] protected UnitInputsComponent unitInputs;
        [SerializeField, Self] protected EntityStatsComponent stats;
        [SerializeField, Child] protected EffectsReceiverComponent effectsReceiver;
        [SerializeField] protected Animator animator;

        [SerializeField] private EnemyData_SO data;
        [SerializeField] string combatGroup;
        public string CombatGroup => combatGroup;
        public Side EntitySide => entity.GetEntitySide;
        public EnemyData_SO Config => data;
        
        private IModifierAggregator _mods;
        private ImpulseApplier _impulse;
        private CombatParticipant _participant;
        
        private bool _paused;
        private float _speedMultiplier = 1f;
        private bool _canMove;
        private bool _canMoveAssigned;
        private bool _behaviorHold;

        #region BLACKBOARD actions

        private bool _inCombat = false;
        private EnterCombatEventChannel _combateventChannel;
        private BlackboardVariable<Vector3> _start;
        private void OnEnable()
        {
            // Достаём/создаём исполнителя импульсов ДО любых return ниже: раньше при пустом CombatState
            // он не создавался, и первый же ApplyImpulse падал с NullReferenceException.
            if (!TryGetComponent(out _impulse)) _impulse = gameObject.AddComponent<ImpulseApplier>();
            _impulse.MotionEnded += OnExternalMotionEnded;

            // участник оркестратора боя (токены атаки, агрессия, слоты)
            if (!TryGetComponent(out _participant)) _participant = gameObject.AddComponent<CombatParticipant>();

            behavior.GetVariable("CombatState", out var combatState);
            _combateventChannel = combatState.ObjectValue as EnterCombatEventChannel;
            if (_combateventChannel == null)
            {
                Debug.Log($"Cast failed! {entity.GetName}");
                return;
            }
            _combateventChannel.Event += OnCombatStateChanged;
            behavior.GetVariable("StartingPosition", out _start);
            
            stats.RegisterStatsViewer(this);
        }

        private void OnDisable()
        {
            if (_impulse != null) _impulse.MotionEnded -= OnExternalMotionEnded;
            if (_combateventChannel != null)
            _combateventChannel.Event -= OnCombatStateChanged;
            stats.UnregisterStatsViewer(this);
        }

        /// <summary>Толчок закончился: возвращаем агенту «намерение» (двигаться / стоять).</summary>
        private void OnExternalMotionEnded(ImpulseApplier _)
        {
            ApplyStopState(ignoreImpulse: true);
        }

        /// <summary>
        /// Граф держит позицию (нода Set navmeshagent pause). В отличие от CanMove, который
        /// машина состояний выставляет при каждой смене состояния, держится, пока граф сам не снимет.
        /// Сбрасывается при смене состояния боя и при смерти/воскрешении.
        /// </summary>
        public bool BehaviorHold
        {
            get => _behaviorHold;
            set
            {
                _behaviorHold = value;
                ApplyStopState();
            }
        }

        /// <summary>
        /// Единственное место, где считается agent.isStopped:
        /// пауза игры, удержание графом, запрет движения от машины состояний.
        /// </summary>
        private void ApplyStopState(bool ignoreImpulse = false)
        {
            if (!ignoreImpulse && ImpulseActive) return; // во время импульса агентом рулит ImpulseApplier, вернёт в OnExternalMotionEnded
            if (!AgentActive) return;
            agent.isStopped = _paused || _behaviorHold || (_canMoveAssigned && !_canMove);
        }

        private void OnCombatStateChanged(bool state, string group)
        {
            if (group != CombatGroup) return;
            bool changed = _inCombat != state;
            _inCombat = state;
            if (!changed) return;
            // ветку атаки могли прервать посреди паузы: не тащим удержание в другое поведение
            _behaviorHold = false;
            ApplyStopState();
            CombatStateChanged?.Invoke(state);
        }

        /// <summary>Враг в бою (по событию своей боевой группы).</summary>
        public bool InCombat => _inCombat;

        /// <summary>Смена состояния боя этого врага.</summary>
        public event Action<bool> CombatStateChanged;

        /// <summary>Сменить боевую группу (призванные берут группу призывателя).</summary>
        public void SetCombatGroup(string group) => combatGroup = group;

        /// <summary>Отправить своей боевой группе событие «вступили в бой».</summary>
        public void ForceEnterCombat()
        {
            if (_combateventChannel != null) _combateventChannel.SendEventMessage(true, combatGroup);
        }
        public NavMeshAgent Nav => agent;
        public float CurrentMoveSpeed  => _inCombat? 
            Config.CombatMoveSpeed * _speedMultiplier :
                    Config.NonCombatMoveSpeed *  _speedMultiplier;
            
        public Vector3 StartPoint => _start.Value;
        public float GetStatPercent(ResourceStatType statType)
        {
            if (stats.TryGetCurrent(statType, out var value))
            {
                return value / stats.GetMax(statType);
            }

            return 0;
        }

        public bool ActionAvailable(UnitActionType actionType)
        {

            var ok = unitInputs.CanPerformCombatAction(new UnitCommand(actionType), out var info);
            if (!ok && entity.ShowingDebugs) Debug.Log(info);
            return ok;
        }

        public bool RequestAction(UnitActionType actiontype)
        {
            return unitInputs.RequestCombatAction(actiontype);
        }

        #endregion

        public bool HasEffect(string ID)
        {
            return effectsReceiver.Controller.HasEffect(ID, out _);
        }
        
        private void LateUpdate()
        {
            if (_mods != null)
            {
                SpeedMultiplier = _mods.GetMultiplier(ModifierParam.MoveSpeed);
            }
        }

#if UNITY_EDITOR
        private Quaternion _debugLastRotation;
        private EntityStateMachineComponent _debugStateMachine;

        /// <summary>
        /// Show Debugs на BaseGameEntity: пишет в консоль, если юнит поворачивается, пока граф держит позицию.
        /// Повороты от ноды Rotate towards тоже попадут в лог — это ожидаемо.
        /// </summary>

#endif

        #region ipausable
        public bool Paused
        {
            get => _paused;
            set
            {
                _paused = value;
                ApplyStopState();

                if (!behavior) return;
                behavior.enabled = !_paused;
            }
        }
        #endregion
        #region ikillable
        
        public void SetKilled(IKillerComponent component, bool value)
        {
            _behaviorHold = false;
            if (value) { if (AgentActive) agent.isStopped = true; }
            else ApplyStopState();
            if (!behavior) return;
            if (value) behavior.End();
            else behavior.Restart();
        }
        #endregion

        #region imover

        public bool ImpulseActive => _impulse != null && _impulse.IsActive;

        public bool CanMove
        {
            get => _canMove;
            set
            {
                _canMove = value;                 // намерение запоминаем ВСЕГДА
                _canMoveAssigned = true;
                ApplyStopState();                 // удержание графом (BehaviorHold) не снимается сменой состояния
            }
        }

        public Vector3 MovementVector
        {
            get => AgentActive ? agent.velocity : Vector3.zero;
            set { if (AgentActive) agent.velocity = value; }
        }

        private bool AgentActive => agent != null && agent.enabled && agent.isOnNavMesh;

        public float ActualMovementVelocity => AgentActive ? agent.velocity.magnitude : 0f;
        public bool IsGrounded => agent.isOnNavMesh;

        public void ApplyMotion(in MotionRequest request)
        {
            if (_impulse == null) return;
            _impulse.Apply(request);
        }
        public bool IsGamepadInput { get; set; } = false;

        public float SpeedMultiplier
        {
            get => _speedMultiplier;
            set
            {
                if (Mathf.Approximately(_speedMultiplier, value)) return;
                _speedMultiplier = value;
            }
        }
        public bool UseRootMotion
        {
            get => animator !=null && animator.applyRootMotion;
            set
            {
                if (agent.enabled && agent.isOnNavMesh)
                {
                    // Выходим из root motion: transform ушёл от внутренней позиции агента.
                    // Без этой синхронизации агент «откатывает» юнита назад к старой точке.
                    if (!value && !agent.updatePosition) agent.nextPosition = transform.position;
                    if (value) agent.velocity = Vector3.zero;
                }
                agent.updatePosition = !value;
                agent.updateRotation = !value;
                if (!animator) return;
                animator.applyRootMotion = value;
            }
        }
        #endregion
        
        #region itier
        public UnitTier GetTierInfo
        {
            get
            {
                bool hasTier = behavior.GetVariable<UnitTier>("Tier", out var tierVar);
                return hasTier ? tierVar.Value : UnitTier.Unassigned;
            }
        }
        #endregion

        #region enter combat
        public void HandleStatsUpdate(ResourceStatType stat, float statCurrent, float statMax, float statDelta, EntityStatsComponent.ExpendType changeType,
            BaseGameEntityComponent source)
        {
            if (source != null && source.GetEntitySide == Side.PlayerSide && statDelta < 0)
            {
                if (_combateventChannel != null) _combateventChannel.SendEventMessage(true,combatGroup);
                if (_participant != null) _participant.OnDamagedByPlayer();
            }
        }

        public void SetShieldValue(ResourceStatType shieldStat, float currentValue)
        {
        }
        #endregion
    }
}