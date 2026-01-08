using Arcatech.Stats;
using Arcatech.Triggers;
using Arcatech.Units.Control;
using KBCore.Refs;
using NUnit.Framework;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

namespace Arcatech.Units
{
    
    [RequireComponent(typeof(BehaviorGraphAgent),typeof(NavMeshAgent),typeof(UnitInputsComponent))]
    [RequireComponent(typeof(BaseGameEntityComponent),typeof(EntityStatsComponent))]
    public class NPCBehaviorWrapper : ValidatedMonoBehaviour, IAppliedEffectsTakerComponent<AppliedStatsDeltaEffect>,IKillableComponent,IPausableComponent,IMove
    {
        [SerializeField,Self]protected NavMeshAgent agent;
        [SerializeField,Self]protected BehaviorGraphAgent behavior;
        [SerializeField,Self]protected BaseGameEntityComponent entity;
        [SerializeField,Self]protected UnitInputsComponent unitInputs;
        [SerializeField,Self]protected EntityStatsComponent stats;

        
        
        private BlackboardReference bbref;
        private readonly string combatEventChannelName = "CombatEnterEvent";
        private readonly string selfRef = "Wrapper";
        
        EnterCombatEventChannel combatEventChannel;
        
        #region BLACKBOARD

        public float GetStatPercent(ResourceStatType statType) => stats.GetCurrent(statType) / stats.GetMax(statType);
        public bool ActionAvailable(UnitActionType actionType) => unitInputs.CanPerformCombatAction(actionType);

        public bool RequestAction(UnitActionType actiontype) => unitInputs.RequestCombatAction(actiontype);

        #endregion
        
        
        protected void Start()
        {
            if (behavior)
            {
                // failsafe for unset units
                bbref = behavior.BlackboardReference;
                bbref.GetVariableValue(combatEventChannelName, out combatEventChannel);
                Assert.IsNotNull(combatEventChannel);
                bbref.SetVariableValue(selfRef,this);
            }
        }
        public bool ApplyEffect(AppliedStatsDeltaEffect effect,BaseGameEntityComponent source)
        {
            if (!behavior) return true;
            if (source == null) return true;
            if (source.GetEntitySide != entity.GetEntitySide && source.GetEntitySide != Side.Unassigned)
            {
                combatEventChannel.SendEventMessage(true);
            }
            return true;
        }

        public void SetKilled(IKillerComponent component, bool value)
        {
            agent.isStopped = value;
            if (!behavior) return;
            if (value) behavior.End(); else behavior.Restart();
        }
        
        private bool _paused;
        public bool Paused
        {
            get => _paused;
            set
            {
                _paused = value;
                // base.OnPause(paused);   
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
        public Vector3 MovementVector { get; set; }
        public float ActualMovementVelocity => agent.velocity.magnitude;
        public bool IsGrounded => agent.isOnNavMesh;
        public bool UseRootMotion { get; set; }

    }
}

