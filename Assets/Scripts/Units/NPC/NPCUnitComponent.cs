using Arcatech.AI;
using KBCore.Refs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Arcatech.Units
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCUnitComponent : ActiveGameUnitComponent
    {

        [Space, Header("Idling settings")]
        protected Transform _player;
        [SerializeField] float _idleWanderRange = 5f;
        [SerializeField] float _waitAtIdleSpotTime = 3f;

        [Space, Header("Patrol settings, uses idle at spot timer")]
        [SerializeField] protected List<NestedList<Transform>> patrolPointVariants;

        [Space, Header("Player seeking setting")]
        [SerializeField] protected float _playerDetectionSphereCastRange = 8f;
        [SerializeField,Range(0.01f,1f), Tooltip("How often enemy scans for player in front of self")] protected float _sphereCastDelay = 0.1f;
        [SerializeField, Range(1, 10f)] protected float _sphereCastRadius = 3f;
        [SerializeField, Range(1, 999)] protected float _combatTimeout = 5f;

        [Space, Header("General combat settings")]
        [SerializeField, Range(0,25)] protected float _attackingRange;

        [Space,SerializeField] SerializedUnitAction _enterCombatAction;
        [SerializeField] SerializedUnitAction _exitCombatAction;

        [SerializeField,Self]protected NavMeshAgent agent;



        protected override void OnValidate()
        {
            base.OnValidate();
            if (GetMainEntity.GetEntitySide== Side.EnemySide && !CompareTag("Enemy"))
            {
                Debug.LogError($"Set enemy tag for {GetMainEntity.GetName}");
            }
        }
        
        //protected override void DamageAction()
        //{
        //    OnUnitAttackedEvent?.Invoke(this);
        //    UnitInCombatState = true;
        //    base.DamageAction();
        //}

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

        #region room tactics

        protected RoomUnitsGroup _group;
        public void SetUnitsGroup(RoomUnitsGroup g) => _group = g;  

        #region combat state
        public event UnityAction<NPCUnitComponent> OnUnitAttackedEvent = delegate { };
        protected CountDownTimer combatTimeoutTimer;
        bool _inCombat = false;
        string _debugString;

        public bool UnitInCombatState
        {
            get
            {
                return _inCombat;
            }
            set
            {
                if (_inCombat == value) return;
                OnCombatStateChanged(value);
                _inCombat = value;
                if (GetMainEntity.ShowingDebugs)
                {
                    Debug.Log($"{GetMainEntity} combat state: {value}");
                }
            }
        }

        protected virtual void OnCombatStateChanged(bool state)
        {
            if (state)
            {
                combatTimeoutTimer ??= new CountDownTimer(_combatTimeout);
                combatTimeoutTimer.Start();
            }
            //if (state && _enterCombatAction != null)
            //{
            //    ForceUnitAction(_enterCombatAction.ProduceAction(this, _headT));
            //}
            //if (!state && _exitCombatAction != null)
            //{
            //    ForceUnitAction(_exitCombatAction.ProduceAction(this, _headT));
            //}
        }

        void InternalCombatStateUpdate(float d)
        {
            SeekPlayer(d);
            combatTimeoutTimer?.Tick(d);
            if (combatTimeoutTimer != null)
            {
                if (combatTimeoutTimer.IsReady)
                {
                    combatTimeoutTimer.Reset(); UnitInCombatState = false; Debug.Log($"combat timeout {GetMainEntity.GetName}");
                };
            }
        }
        RaycastHit[] hits = new RaycastHit[20];

        float _castDelay = 0f;
        void SeekPlayer(float delta)
        {
            //_castDelay += delta;
            //if (_castDelay >= _sphereCastDelay)
            //{
            //    _castDelay = 0f;
            //    if (Physics.SphereCastNonAlloc(_headT.position, _sphereCastRadius, transform.forward, hits, _playerDetectionSphereCastRange) > 0)
            //    {
            //        foreach (RaycastHit hit in hits)
            //        {
            //            if (hit.collider != null && hit.collider.CompareTag("Player"))
            //            {
            //                combatTimeoutTimer?.Reset();
            //                UnitInCombatState = true;
            //                //if (_showDebugs) Debug.Log($"{UnitName} spotted player!");
            //                break;
            //            }
            //        }
            //    }
            //}
        }


        #endregion

        protected bool CheckDistance (Transform t, Comparer c, float value)
        {
            float d = Vector3.Distance(transform.position, t.position);
            switch (c)
            {
                case Comparer.Equal:
                    return d == value;

                case Comparer.NotEqual:
                    return d!= value;
                case Comparer.Greater:
                    return d > value;
                case Comparer.Less:
                    return d < value;   
            }
            return false;
        }

        #endregion
    }
}

