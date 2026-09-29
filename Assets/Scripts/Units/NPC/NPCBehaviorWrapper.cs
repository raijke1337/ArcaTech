using System;
using Arcatech.SaveSystem;
using Arcatech.Stats;
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
        ITierProvider
    {
        [SerializeField, Self] protected NavMeshAgent agent;
        [SerializeField, Self] protected BehaviorGraphAgent behavior;
        [SerializeField, Self] protected BaseGameEntityComponent entity;
        [SerializeField, Self] protected UnitInputsComponent unitInputs;
        [SerializeField, Self] protected EntityStatsComponent stats;
        [SerializeField, Child] protected EffectsReceiverComponent effectsReceiver;
        [SerializeField] protected Animator animator;

        [SerializeField] private EnemyData_SO data;
        [SerializeField] string CombatGroup;
        public Side EntitySide => entity.GetEntitySide;
        public EnemyData_SO Config => data;
        
        private IModifierAggregator _mods;
        private ImpulseApplier _impulse;
        
        private bool _paused;
        private float _speedMultiplier = 1f;
        private bool _canMove;
        private bool _canMoveAssigned;

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

            behavior.GetVariable("CombatState", out var combatState);
            _combateventChannel = combatState.ObjectValue as EnterCombatEventChannel;
            if (_combateventChannel == null)
            {
                Debug.Log($"Cast failed! {entity.GetName}");
                return;
            }
            _combateventChannel.Event += OnCombatStateChanged;
            behavior.GetVariable("StartingPosition", out _start);
        }

        private void OnDisable()
        {
            if (_impulse != null) _impulse.MotionEnded -= OnExternalMotionEnded;
            if (_combateventChannel != null)
            _combateventChannel.Event -= OnCombatStateChanged;
        }

        /// <summary>Толчок закончился: возвращаем агенту «намерение» (двигаться / стоять).</summary>
        private void OnExternalMotionEnded(ImpulseApplier _)
        {
            if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
            agent.isStopped = _paused || (_canMoveAssigned && !_canMove);
        }
        private void OnCombatStateChanged(bool state) => _inCombat =  state;
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

        #region ipausable
        public bool Paused
        {
            get => _paused;
            set
            {
                _paused = value;
                if (agent.enabled && agent.isOnNavMesh) agent.isStopped = _paused;

                if (!behavior) return;
                behavior.enabled = !_paused;
            }
        }
        #endregion
        #region ikillable
        
        public void SetKilled(IKillerComponent component, bool value)
        {
            if (agent.isOnNavMesh) agent.isStopped = value;
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
                if (ImpulseActive) return;        // во время импульса агентом рулит ImpulseApplier (вернёт isStopped в OnExternalMotionEnded)
                if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
                agent.isStopped = !value;
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
        
    }
}