using Arcatech.Triggers;
using KBCore.Refs;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

namespace Arcatech.Units
{
    [RequireComponent(typeof(BehaviorGraphAgent),typeof(NavMeshAgent))]
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
            set
            {
                combatEventChannel.SendEventMessage(value);
            }
        }

         
        
        override protected void Start()
        {
            base.Start();
            bbref = behavior.BlackboardReference;
            bbref.GetVariableValue("PlayerAttackedEvent", out combatEventChannel);
        }
        
        protected override void OnActionLock(bool locking)
        {
            agent.isStopped = locking;
        }

        protected override void OnPause(bool paused)
        {
            agent.isStopped = paused;
            behavior.enabled = !paused;
        }

        public override void Kill()
        {
            base.Kill();
            agent.isStopped = true;
            behavior.End();
        }

        public void ApplyEffect(StatsEffect effect,BaseGameEntityComponent source)
        {
            if (source.GetEntitySide != GetMainEntity.GetEntitySide && GetMainEntity.GetEntitySide != Side.Unassigned)
            {
                CombatState = true;
               // Debug.Log($"{GetMainEntity.GetName} received {effect} from {source}, entering combat");
            }
        }
    }
}

