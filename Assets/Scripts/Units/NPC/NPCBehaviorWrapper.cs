
using System;
using Arcatech.SaveSystem;
using Arcatech.Stats;
using Arcatech.Units.Control;
using Arcatech.Usables.Effects;
using KBCore.Refs;
using NUnit.Framework.Constraints;
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

        #region BLACKBOARD actions

        private bool _inCombat = false;
        private EnterCombatEventChannel _combateventChannel;
        private BlackboardVariable<Vector3> _start;
        private void OnEnable()
        {
            behavior.GetVariable("CombatState", out var combatState);
            _combateventChannel = combatState.ObjectValue as EnterCombatEventChannel;
            if (_combateventChannel == null)
            {
                Debug.Log($"Cast failed! {entity.GetName}");
                return;
            }
            _combateventChannel.Event += OnCombatStateChanged;
            behavior.GetVariable("StartingPosition", out _start);
            if (!TryGetComponent(out _impulse)) _impulse = gameObject.AddComponent<ImpulseApplier>();
        }

        private void OnDisable()
        {
            if (_combateventChannel != null)
            _combateventChannel.Event -= OnCombatStateChanged;
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
                agent.isStopped = _paused;

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
                if (ImpulseActive) return;        // во время импульса агентом рулит физика
                if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
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
        public void ApplyImpulse(Vector3 impulse) => _impulse.ApplyImpulse(impulse);
        public void ApplyImpulse(float impulseRelative)=>  _impulse.ApplyImpulse(impulseRelative);
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
                agent.updatePosition = !value;
                agent.updateRotation = !value;
                if (value) agent.velocity = Vector3.zero;
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

