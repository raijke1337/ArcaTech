using Arcatech.Items;
using Arcatech.Stat;
using Arcatech.Units;
using DG.Tweening;
using ECM.Components;
using KBCore.Refs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace Arcatech
{
    /// <summary>
    /// new component to define a unit that has stats (can be attacked) and does actions
    /// </summary>
    [RequireComponent(typeof(BaseGameEntityComponent), typeof(Animator), typeof(EntityStatsComponent))]

    public class ActiveGameUnitComponent : ValidatedMonoBehaviour
    {
        [SerializeField, Self] BaseGameEntityComponent gameEntity;
        [SerializeField, Self] protected Animator _animator;
        [SerializeField, Self] protected EntityStatsComponent _stats;
        

        [Space, SerializeField] protected SerializedUnitAction ActionOnDamage;
        [SerializeField] protected SerializedUnitAction ActionOnDeath;
        public BaseGameEntityComponent GetMainEntity { get => gameEntity; }
        public Animator GetAnimatorReference => _animator;
        


        protected virtual void Start()
        {
            if (TryGetComponent<EntityInventoryComponent>(out var inv))
            {
                AssignActionsHandler(inv.GetUnitActionsHandler);
            }
            else
            {
                Debug.LogWarning($"{GetMainEntity.GetName} has no actions handler assigned at startup because it has no inventory");
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

        private void Update()
        {

            if (currentAction != null)
            {
                switch (currentAction?.UpdateAction(Time.deltaTime))
                {
                    case UnitActionState.None:
                        break;
                    case UnitActionState.Started:
                        ActionLock = currentAction.LockMovement;
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

        #region actions

        List<IUnitActionsHandler> _actionsHandlers;

        public event UnityAction <UnitActionType> DidActionAnnounceEvent = delegate { };

        protected BaseUnitAction currentAction;
        /// <summary>
        /// assign some other handler that isnt the one in inventory
        /// </summary>
        public virtual void AssignActionsHandler(IUnitActionsHandler handler)
        { 
            if (_actionsHandlers == null) _actionsHandlers = new List<IUnitActionsHandler>();
            _actionsHandlers.Add(handler);
        }


        public virtual void Command(UnitActionType obj)
        {
            if (_lockAction ||  GetMainEntity.Paused || !CanAct()) return;

            foreach (var h in _actionsHandlers)
            {
                if (h.TryHandleAction(obj, _stats, out var a))
                {
                    DoActionLogic(a);
                    DidActionAnnounceEvent?.Invoke(obj);
                }
            }
        }


        protected virtual bool CanAct()
        {
            /// extra checks in npc and player
            return true;
        }

        public void ForceUnitAction(BaseUnitAction act)
        {
            if (gameEntity.Paused || act == null) return;
            OnForceAction(act);
        }
        protected virtual void OnForceAction(BaseUnitAction act)
        {
            DoActionLogic(act);
        }
        protected void DoActionLogic(BaseUnitAction act)
        {
            if (act == null) return;
            if (currentAction != null && currentAction != act && currentAction.GetActionState != UnitActionState.Completed)
            {
                currentAction.CompleteAction();
            }
            currentAction = act;
            ActionLock = currentAction.LockMovement;
            currentAction.StartAction();
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
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                //if (_showDebugs) Debug.Log("Boom");
                force?.Kill();
            }
        }
        #endregion
    }
}