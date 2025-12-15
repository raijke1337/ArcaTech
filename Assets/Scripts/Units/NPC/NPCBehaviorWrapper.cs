using Arcatech.Stats;
using Arcatech.Triggers;
using KBCore.Refs;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

namespace Arcatech.Units
{
    /// <summary>
    /// this is now separate from ActiveUnitComp (aka StateMachine)
    /// </summary>
    [RequireComponent(typeof(BehaviorGraphAgent),typeof(NavMeshAgent),typeof(UnitInputsComponent))]
    [RequireComponent(typeof(EntityStateMachineComponent))]
    public class NPCBehaviorWrapper : ValidatedMonoBehaviour, IEffectsTakerComponent
    {
        
        [SerializeField,Self]protected NavMeshAgent agent;
        [SerializeField,Self]protected BehaviorGraphAgent behavior;
        [SerializeField,Self]protected EntityStateMachineComponent stateMachine;
        [SerializeField,Self]protected UnitInputsComponent unitInputs;
        
        private BlackboardReference bbref;
        EnterCombatEventChannel combatEventChannel;

        public bool CombatState
        {
            get
            {
                bbref.GetVariableValue("IsInCombat", out bool result);
                return result;  
            }
            set => combatEventChannel.SendEventMessage(value);
        }

         
        
        protected void Start()
        {
            if (!behavior) return;
            // failsafe for unset units
            bbref = behavior.BlackboardReference;
            bbref.GetVariableValue("PlayerAttackedEvent", out combatEventChannel);
        }
        
        protected void OnActionLock(bool locking)
        {
          //  base.OnActionLock(locking);
            agent.isStopped = locking;
        }

        protected void OnPause(bool paused)
        {
           // base.OnPause(paused);   
            agent.isStopped = paused;
            
            if (!behavior) return;
            behavior.enabled = !paused;
        }

        protected void OnKill(bool kill)
        {
           // base.OnKill(kill);
            agent.isStopped = true;
            
            if (!behavior) return;
            behavior.End();
        }



        public void ApplyEffect(UsableEffect effect,BaseGameEntityComponent source)
        {
            if (source == null) return;
            if (source.GetEntitySide != stateMachine.GetMainEntity.GetEntitySide && stateMachine.GetMainEntity.GetEntitySide != Side.Unassigned)
            {
                CombatState = true;
               // Debug.Log($"{GetMainEntity.GetName} received {effect} from {source}, entering combat");
            }
        }
    }
}

