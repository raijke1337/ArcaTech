using Arcatech.AI;
using Arcatech.Triggers;
using Arcatech.Units.Behaviour;
using KBCore.Refs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Arcatech.Units
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCUnit : ControlledUnit, IRoomUnitTacticsMember
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

        protected virtual void OnDrawGizmos()
        {
            if (_showDebugs)
            {
                // combat state and detection
                Gizmos.color = Color.green;
                UnityEditor.Handles.color = Color.green;
                if (UnitInCombatState)
                {
                    Gizmos.color = Color.yellow;
                    UnityEditor.Handles.color = Color.yellow;
                }
                Gizmos.DrawLine(_headT.position, _headT.position + (_headT.forward * _playerDetectionSphereCastRange));

                UnityEditor.Handles.DrawWireDisc(_headT.position, Vector3.up, _sphereCastRadius);
                UnityEditor.Handles.DrawWireDisc(_headT.position + (_headT.forward * _playerDetectionSphereCastRange), Vector3.up, _sphereCastRadius);

                if (agent != null && agent.destination != null)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(agent.destination, 0.3f);
                    UnityEditor.Handles.color = Color.blue;
                    UnityEditor.Handles.DrawWireDisc(transform.position,Vector3.up,agent.stoppingDistance);
                }

                UnityEditor.Handles.color = Color.red;
                UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, _attackingRange);
            }
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (this.Side == Side.EnemySide && !CompareTag("Enemy"))
            {
                Debug.LogError($"Set enemy tag for {UnitName}");
            }
        }
        
        protected override void DamageAction()
        {
            OnUnitAttackedEvent?.Invoke(this);
            UnitInCombatState = true;
            base.DamageAction();
        }

        protected override void HandleInteractionAction(IInteractible i)
        {
            if (_showDebugs) Debug.Log($"{UnitName} wants to interact with {i}");
            i.AcceptInteraction(this);
        }
        protected override void OnActionLock(bool locking)
        {
            agent.isStopped = locking;
            if (locking) _animator.SetBool("isMoving", false);
        }
        protected override void OnUnitPause(bool isPause)
        {
            if (agent != null)
            {
                agent.isStopped = isPause;
            } // wtf....
            if (_animator != null) _animator.SetBool("isMoving", !isPause);
        }

        public override void StartControllerUnit()
        {
            base.StartControllerUnit();
            agent.speed = movementStats.Stats[Stats.MovementStatType.Movespeed].Max;
            agent.updateRotation = true; // todo check what this adoes

            _player = FindObjectOfType<PlayerUnit>().transform;
            BaseBehaviourSetup();
            SetupBehavior();
        }

        public override void RunUpdate(float delta)
        {
            base.RunUpdate(delta);
            _ground.DetectGround();
            InternalCombatStateUpdate(delta);
            _animator.SetBool("isMoving", agent.velocity.magnitude > 0 && !agent.isStopped);
            ExecuteBehaviour();
        }

        #region behavior


        protected BehaviourTree tree;
        [Space, SerializeField, Self] protected NavMeshAgent agent;


        protected float initStoppingDistance;

        void BaseBehaviourSetup()
        {
            tree = new BehaviourTree(UnitName + " undefined behavior tree");

            initStoppingDistance = agent.stoppingDistance;
        }
        protected virtual void SetupBehavior()
        {
            BehaviourPrioritySelector dummyUnitBehavior = new BehaviourPrioritySelector("npc idles");
            if (patrolPointVariants != null && patrolPointVariants.Count > 0)
            {
                dummyUnitBehavior.AddChild(SetupPatrolPoints());
            }
            dummyUnitBehavior.AddChild(SetupIdleRoaming());

            tree.AddChild(dummyUnitBehavior);

            Debug.Log($"{UnitName} is using dummy behavior!");
        }
        void ExecuteBehaviour()
        {
            if (ActionLock || UnitPaused) return;
            if (tree?.Process(this) == Node.NodeStatus.Fail)
            {
                Debug.Log($"{UnitName} has an empty behavior tree!");
            }
        }
        /// <summary>
        /// priority 0
        /// </summary>
        /// <returns>0 priority idling sequence</returns>
        protected Sequence SetupIdleRoaming()
        {
            Sequence roamandWait = new Sequence("walk around and wait",0);
            Leaf roam = new Leaf(new RoamAroundPoint(_idleWanderRange, transform.position,agent), $"roam in range {_idleWanderRange}");

            Sequence idleWait = new("idle for time");
            Leaf wait = new(new IdleAtPointStrategy(_waitAtIdleSpotTime), $"Wait {_waitAtIdleSpotTime} sec");
            idleWait.AddChild(wait);

            bool isDoneWaiting()
            {
                if (wait.Process(this) == Node.NodeStatus.Success)
                {
                    roamandWait.Reset();
                    return true;
                }
                else return false;
            }
            Leaf checkWait = new Leaf(new SimpleBehaviourCondition(() => isDoneWaiting()),"is finished idle time");
            idleWait.AddChild(checkWait);

            roamandWait.AddChild(roam);
            roamandWait.AddChild(idleWait);

            return roamandWait;
        }

        protected Sequence SetupPatrolPoints()
        {
            Sequence randomPatrol = new Sequence("patrol points", 20);
            List<Transform> points = patrolPointVariants[0].InternalList; // todo pick random

            randomPatrol.AddChild(new Leaf(new PatrolPointsStrategy(transform, agent, points, _waitAtIdleSpotTime), "patrol points", 30));

            return randomPatrol;
        }


        #endregion

        #region room tactics

        protected RoomUnitsGroup _group;
        public void SetUnitsGroup(RoomUnitsGroup g) => _group = g;  

        #region combat state
        public event UnityAction<NPCUnit> OnUnitAttackedEvent = delegate { };
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
                tree.Reset();
                if (_showDebugs)
                {
                    Debug.Log($"{UnitName} combat state: {value}");
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
            if (state && _enterCombatAction != null)
            {
                ForceUnitAction(_enterCombatAction.ProduceAction(this, _headT));
            }
            if (!state && _exitCombatAction != null)
            {
                ForceUnitAction(_exitCombatAction.ProduceAction(this, _headT));
            }
        }

        void InternalCombatStateUpdate(float d)
        {
            SeekPlayer(d);
            combatTimeoutTimer?.Tick(d);
            if (combatTimeoutTimer != null)
            {
                if (combatTimeoutTimer.IsReady)
                {
                    combatTimeoutTimer.Reset(); UnitInCombatState = false; Debug.Log($"combat timeout {UnitName}");
                };
            }
        }
        RaycastHit[] hits = new RaycastHit[20];

        float _castDelay = 0f;
        void SeekPlayer(float delta)
        {
            _castDelay += delta;
            if (_castDelay >= _sphereCastDelay)
            {
                _castDelay = 0f;
                if (Physics.SphereCastNonAlloc(_headT.position, _sphereCastRadius, transform.forward, hits, _playerDetectionSphereCastRange) > 0)
                {
                    foreach (RaycastHit hit in hits)
                    {
                        if (hit.collider != null && hit.collider.CompareTag("Player"))
                        {
                            combatTimeoutTimer?.Reset();
                            UnitInCombatState = true;
                            //if (_showDebugs) Debug.Log($"{UnitName} spotted player!");
                            break;
                        }
                    }
                }
            }
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

