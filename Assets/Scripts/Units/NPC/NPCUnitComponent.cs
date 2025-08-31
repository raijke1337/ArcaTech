using KBCore.Refs;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

namespace Arcatech.Units
{
    [RequireComponent(typeof(BehaviorGraphAgent),typeof(NavMeshAgent))]
    public class NPCUnitComponent : ActiveGameUnitComponent
    {

        //[Space, Header("Idling settings")]
        //protected Transform _player;
        //[SerializeField] float _idleWanderRange = 5f;
        //[SerializeField] float _waitAtIdleSpotTime = 3f;

        //[Space, Header("Patrol settings, uses idle at spot timer")]
        //[SerializeField] protected List<NestedList<Transform>> patrolPointVariants;

        //[Space, Header("Player seeking setting")]
        //[SerializeField] protected float _playerDetectionSphereCastRange = 8f;
        //[SerializeField,Range(0.01f,1f), Tooltip("How often enemy scans for player in front of self")] protected float _sphereCastDelay = 0.1f;
        //[SerializeField, Range(1, 10f)] protected float _sphereCastRadius = 3f;
        //[SerializeField, Range(1, 999)] protected float _combatTimeout = 5f;

        //[Space, Header("General combat settings")]
        //[SerializeField, Range(0,25)] protected float _attackingRange;

        //[Space,SerializeField] SerializedUnitAction _enterCombatAction;
        //[SerializeField] SerializedUnitAction _exitCombatAction;

        [SerializeField,Self]protected NavMeshAgent agent;
        [SerializeField,Self]protected BehaviorGraphAgent behavior;

        protected override void OnActionLock(bool locking)
        {
           agent.isStopped = locking;

           _animator.SetBool("isMoving", false);
        }


        //protected override void OnUnitPause(bool isPause)
        //{
        //    if (agent != null)
        //    {
        //        agent.isStopped = isPause;
        //    } // wtf....
        //    if (_animator != null) _animator.SetBool("isMoving", !isPause);
        //}

        //public override void StartControllerUnit()
        //{
        //    base.StartControllerUnit();
        //    agent.speed = movementStats.Stats[Stats.MovementStatType.Movespeed];
        //    agent.updateRotation = true; // todo check what this adoes

        //    _player = FindObjectOfType<PlayerUnit>().transform;
        //    BaseBehaviourSetup();
        //    SetupBehavior();
        //}

        //public override void RunUpdate(float delta)
        //{
        //    base.RunUpdate(delta);
        //    _ground.DetectGround();
        //    InternalCombatStateUpdate(delta);
        //    //_animator.SetBool("isMoving", agent.velocity.magnitude > 0 && !agent.isStopped);
        //    ExecuteBehaviour();
        //}

        #region behavior
        /// <summary>
        /// all behavior to be moved into unity behavior syustem
        /// </summary>

        #endregion

   }
}

