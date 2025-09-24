using KBCore.Refs;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

namespace Arcatech.Units
{
    [RequireComponent(typeof(BehaviorGraphAgent),typeof(NavMeshAgent))]
    public class NPCUnitComponent : ActiveGameUnitComponent
    {
        
        [SerializeField,Self]protected NavMeshAgent agent;
        [SerializeField,Self]protected BehaviorGraphAgent behavior;

        protected override void OnActionLock(bool locking)
        {
           agent.isStopped = locking;

           animator.SetBool("isMoving", false);
        }

        public override void Kill()
        {
            base.Kill();
            agent.isStopped = true;
            behavior.End();
        }
    }
}

