using Arcatech.EventBus;
using Arcatech.Items;
using Arcatech.Stats;
using Arcatech.Units;
using DG.Tweening;
using KBCore.Refs;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
namespace Arcatech
{
    /// <summary>
    /// new component to define a unit that has some state (e.g. idle, attacking, stunned...)
    /// </summary>
    [RequireComponent(typeof(BaseGameEntityComponent),typeof(EntityStatsComponent))]

    public class ActiveGameUnitComponent : ValidatedMonoBehaviour, IPausableComponent,IKillableComponent
    {
        [SerializeField, Self] BaseGameEntityComponent gameEntity;
        [SerializeField, Child] protected Animator animator;
        [SerializeField, Self] protected EntityStatsComponent _stats;


        [Space, SerializeField] protected SerializedUnitState StaggeredState;
        [SerializeField] protected SerializedUnitState DeadState;
        [SerializeField] protected SerializedUnitState StunnedState;

        UnitState _staggerState;
        UnitState _deathState;
        UnitState _stunnedState;
        
        SimpleEntityShadowComponent _entityShadowComponent;

        public BaseGameEntityComponent GetMainEntity { get => gameEntity; }


        protected virtual void Start()
        {
            
            var commandHandlers = GetComponentsInChildren<IUnitCommandHandler>();
            if (commandHandlers.Length == 0)
            {
                Debug.Log($"No unit command handlers found {GetMainEntity.GetName}");
            }
            foreach (var handler in commandHandlers)
            {
                AssignActionsHandler(handler);
            }
            
            if (StaggeredState) _staggerState = StaggeredState.DeserializeState(this,transform);
            if (DeadState) _deathState = DeadState.DeserializeState(this, transform);
            if (StunnedState) _stunnedState = StunnedState.DeserializeState(this,transform);

        }

        private void Update()
        {
            if (Paused) return;

            if (CurrentState != null)
            {
                switch (CurrentState.UpdateAction(Time.deltaTime))
                {
                    case UnitActionState.None:
                        break;
                    case UnitActionState.Started:
                        ActionLock = CurrentState.LockMovement;
                        break;
                    case UnitActionState.ExitTime:
                        ActionLock = false;
                        break;
                    case UnitActionState.Completed:
                        ActionLock = false;
                        break;
                }
            }
        }


        #region locks
        bool _lockAction;
        public bool ActionLock
        {
            get => _lockAction;
            protected set
            {
                OnActionLock(value);
                _lockAction = value;
            }
        }

        protected virtual void OnActionLock(bool locking) { } // do something if needed
        #endregion

        /// <summary>
        /// TODO replace this with states with transitions
        /// ie Idle state, Movement state, Attacking (UnitAction) state
        /// </summary>

        #region actions

        private List<IUnitCommandHandler> _actionsHandlers = new List<IUnitCommandHandler>();
        protected UnitState CurrentState;
        /// <summary>
        /// assign some other handler that isn't attached to the gameobject
        /// </summary>
        public virtual void AssignActionsHandler(IUnitCommandHandler handler)
        { 
            if (!_actionsHandlers.Contains(handler)) _actionsHandlers.Add(handler);
            else Debug.LogWarning($"Tried to assigned the same handler {handler} twice");
        }


        public virtual bool Command(UnitActionType obj)
        {
            bool allSuccess = true;
            
            if (_lockAction ||  Paused || !CanAct()) return false;

            foreach (var h in _actionsHandlers)
            {
                if (h.TryHandleUnitCommand(obj, _stats, out var a))
                {
                    DoActionLogic(a);
                }
                else
                {
                    allSuccess = false;
                }
            }
            return allSuccess;
        }


        protected virtual bool CanAct()
        {
            // extra checks in npc and player
            return true;
        }

        public void ForceUnitState(UnitState act)
        {
            if (Paused || act == null) return;
            DoActionLogic(act);
        }

        protected void DoActionLogic(UnitState act)
        {
            if (act == null) return;
            if (CurrentState != null && CurrentState != act && CurrentState.GetActionState != UnitActionState.Completed)
            {
                CurrentState.ExitState();
            }
            CurrentState = act;
            ActionLock = CurrentState.LockMovement;
            
            CurrentState.StartState();
            
        }

        #endregion
        #region force
        Tweener force;
        public virtual void ApplyForceResultToUnit(float speed, float distance)
        {
            if (gameObject.TryGetComponent<Rigidbody>(out var rb))
            {
                Vector3 end = rb.transform.position + (rb.transform.forward * distance);
                force = rb.DOMove(end, Mathf.Abs(distance / speed), false);
            }
            else
            {
                Debug.Log($"Tried to apply impulse {distance} to {gameEntity.GetName} but it has no rigidbody");
            }
        }

        #endregion

        #region ipausable

        private bool _p;

        public bool Paused
        {
            get => _p;
            set
            {
                _p = value;
                OnPause(value);
            }
        }

        protected virtual void OnPause(bool paused)
        {
            // do something
        }
        
        #endregion
        

        #region IKillable

        private bool _k;

        public bool Killed
        {
            get => _k;
            set => OnKill(value);
        }

        protected virtual void OnKill(bool kill)
        {
            Debug.Log($"{GetMainEntity.GetName} died");
            Paused = kill;
            _k = kill;
            _deathState?.StartState(); 
            if (!_k) Debug.Log($"Trying to resurrect {this} and its NYI. You can't bring back the dead...");
        }

        #endregion
    }
}