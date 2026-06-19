using System;
using Arcatech.SaveSystem;
using Arcatech.Stats;
using Arcatech.Units.Control;
using Arcatech.Usables.Effects;
using KBCore.Refs;
using NUnit.Framework;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

namespace Arcatech.Units
{
    [RequireComponent(typeof(BehaviorGraphAgent), typeof(NavMeshAgent), typeof(UnitInputsComponent))]
    [RequireComponent(typeof(BaseGameEntityComponent), typeof(EntityStatsComponent))]
    [RequireComponent(typeof(EffectsReceiverComponent))]
    public class NPCBehaviorWrapper : ValidatedMonoBehaviour, IKillableComponent, IPausableComponent, IMove,
        ISavedProgressItem
    {
        [SerializeField, Self] protected NavMeshAgent agent;
        [SerializeField, Self] protected BehaviorGraphAgent behavior;
        [SerializeField, Self] protected BaseGameEntityComponent entity;
        [SerializeField, Self] protected UnitInputsComponent unitInputs;
        [SerializeField, Self] protected EntityStatsComponent stats;
        [SerializeField, Child] protected Animator animator;
        [SerializeField, Child] protected EffectsReceiverComponent effectsReceiver;



        #region BLACKBOARD actions

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

            var ok = unitInputs.CanPerformCombatAction(actionType, out var info);
            if (!ok && entity.ShowingDebugs) Debug.Log(info);
            return ok;
        }

        public bool RequestAction(UnitActionType actiontype)
        {
            return unitInputs.RequestCombatAction(actiontype);
        }

        #endregion

        private void Start()
        {

            Initialize();
        }

        public bool HasEffect(string ID)
        {
            return effectsReceiver.Controller.HasEffect(ID, out _);
        }

        public void SetKilled(IKillerComponent component, bool value)
        {
            agent.isStopped = value;
            ReadItemState = value ? ProgressItemState.Completed : ProgressItemState.Default;

            if (!behavior) return;
            if (value) behavior.End();
            else behavior.Restart();
        }

        private bool _paused;

        public bool Paused
        {
            get => _paused;
            set
            {
                _paused = value;
                agent.isStopped = _paused;

                if (!behavior) return;
                behavior.enabled = !_paused;
            }
        }

        private bool _canMove;

        public bool CanMove
        {
            get => _canMove;
            set
            {
                _canMove = value;
                agent.isStopped = !value;
            }
        }

        public Vector3 MovementVector
        {
            get => agent.velocity;
            set => agent.velocity = value;
        }

        public float ActualMovementVelocity => agent.velocity.magnitude;
        public bool IsGrounded => agent.isOnNavMesh;

        public bool UseRootMotion
        {
            get => animator.applyRootMotion;
            set
            {
                animator.applyRootMotion = value;
                agent.updatePosition = !value;
                agent.updateRotation = !value;
                if (value) agent.velocity = Vector3.zero;
            }
        }

        private IModifierAggregator _mods;

        private void LateUpdate()
        {
            if (_mods != null)
            {
                SpeedMultiplier = _mods.GetMultiplier(ModifierParam.MoveSpeed);
            }
        }

        #region save

        public string SavedItemID => entity.GetID;

        public void ApplySaveState(ProgressItemState state, LevelProgressManager ctx)
        {
            if (state == ProgressItemState.Completed)
            {
                gameObject.SetActive(false);
            }
        }

        public string Name => entity.GetName;

        private ProgressItemState _currentState = ProgressItemState.Default;

        public ProgressItemState ReadItemState
        {
            get => _currentState;
            set
            {
                _currentState = value;
                LevelProgressManager.Instance.SavedItemAnnounce(this);
            }
        }

        #endregion

        private BlackboardVariable<float> m_NonCombatMSVar;
        private BlackboardVariable<float> m_CombatMSVar;
        
        private float m_OriginalNonCombatMS;
        private float m_OriginalCombatMS;
        private bool m_IsInitialized = false;
        
        private float _speedMultiplier = 1f;
        public float SpeedMultiplier
        {
            get => _speedMultiplier;
            set
            {
                if (Mathf.Approximately(_speedMultiplier, value)) return;
                _speedMultiplier = value;

                // Если кто-то пытается применить дебафф раньше, чем отработал Start()
                if (!m_IsInitialized)
                {
                    Initialize(); 
                }
                else
                {
                    ApplySpeedMultiplier();
                }
            }
        }
        
        private void Initialize()
        {
            if (m_IsInitialized) return;

            effectsReceiver.TryGetModifierAggregator(out _mods);
            // BehaviorGraphAgent.GetVariable автоматически ищет во всей иерархии черных досок, 
            // включая доп. ассеты (такие как EnemyData), подключенные к графу.
            bool hasNonCombat = behavior.GetVariable<float>("NonCombatMS", out m_NonCombatMSVar);
            bool hasCombat = behavior.GetVariable<float>("CombatMS", out m_CombatMSVar);

            if (hasNonCombat && hasCombat)
            {
                // Кешируем стартовые значения из Blackboard на момент старта игры
                m_OriginalNonCombatMS = m_NonCombatMSVar.Value;
                m_OriginalCombatMS = m_CombatMSVar.Value;
                m_IsInitialized = true;

                // Сразу применяем множитель (на случай, если его изменили до вызова Start)
                ApplySpeedMultiplier();
            }
            else
            {
                Debug.LogWarning($"[SpeedController] Переменные 'NonCombatMS' и 'CombatMS' не найдены в доске на {gameObject.name}. " +
                                 $"Убедитесь, что граф инициализирован и доска подключена.");
            }
        }
        private void ApplySpeedMultiplier()
        {
            if (!m_IsInitialized) return;

            // Расчет итоговой рабочей скорости по формуле: 
            // $V_{current} = V_{original} \times SpeedMultiplier$
            m_NonCombatMSVar.Value = m_OriginalNonCombatMS * _speedMultiplier;
            m_CombatMSVar.Value = m_OriginalCombatMS * _speedMultiplier;
        }

    }
}

