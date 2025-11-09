using Arcatech.Stats;
using Arcatech.Triggers;
using KBCore.Refs;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

namespace Arcatech.Units
{
    [RequireComponent(typeof(BehaviorGraphAgent),typeof(NavMeshAgent),typeof(UnitInputsComponent))]
    public class NPCUnitComponent : ActiveGameUnitComponent, IEffectsTakerComponent
    {
        
        [SerializeField,Self]protected NavMeshAgent agent;
        [SerializeField,Self]protected BehaviorGraphAgent behavior;
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

         
        
        protected override void Start()
        {
            base.Start();
            if (!behavior) return;
            // failsafe for unset units
            bbref = behavior.BlackboardReference;
            bbref.GetVariableValue("PlayerAttackedEvent", out combatEventChannel);
        }
        
        protected override void OnActionLock(bool locking)
        {
            base.OnActionLock(locking);
            agent.isStopped = locking;
        }

        protected override void OnPause(bool paused)
        {
            base.OnPause(paused);   
            agent.isStopped = paused;
            
            if (!behavior) return;
            behavior.enabled = !paused;
        }

        protected override void OnKill(bool kill)
        {
            base.OnKill(kill);
            agent.isStopped = true;
            
            if (!behavior) return;
            behavior.End();
        }



        public void ApplyEffect(StatsEffect effect,BaseGameEntityComponent source)
        {
            if (source == null) return;
            if (source.GetEntitySide != GetMainEntity.GetEntitySide && GetMainEntity.GetEntitySide != Side.Unassigned)
            {
                CombatState = true;
               // Debug.Log($"{GetMainEntity.GetName} received {effect} from {source}, entering combat");
            }
        }
    }
}

