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
    /// new component to define a unit that has stats (can be damaged and killed)
    /// </summary>
    [RequireComponent(typeof(BaseGameEntityComponent),typeof(EntityStatsComponent))]

    public class ActiveGameUnitComponent : ValidatedMonoBehaviour, IStatUpdatesHandler, IPausableComponent,IKillableComponent
    {
        [SerializeField, Self] BaseGameEntityComponent gameEntity;
        [SerializeField, Child] protected Animator animator;
        [SerializeField, Self] protected EntityStatsComponent _stats;


        [Space, SerializeField] protected SerializedUnitAction ActionOnDamage;
        [SerializeField] protected SerializedUnitAction ActionOnDeath;

        BaseUnitAction _damageAction;
        BaseUnitAction _deathAction;
        
        SimpleEntityShadowComponent _entityShadowComponent;

        public BaseGameEntityComponent GetMainEntity { get => gameEntity; }
        public Animator GetAnimatorReference => animator;

        [Space, Header("Stats changes handlers")]
        [SerializeField] StatsUpdateStrategy[] statsUpdateStrategies;
        IOnStatsChangeStrategy[] _statsStrats;

        protected virtual void Start()
        {
            if (TryGetComponent<EntityInventoryComponent>(out var inv))
            {
                AssignActionsHandler(inv.GetUnitActionsHandler);
                inv.SetModelView(_stats);
            }
            else
            {
                Debug.LogWarning($"{GetMainEntity.GetName} has no actions handler assigned at startup because it has no inventory");
            }
            _stats.RegisterStatChangesHandler(this);

            _damageAction = ActionOnDamage.ProduceAction(this,transform);
            _deathAction = ActionOnDeath.ProduceAction(this, transform);


            if (statsUpdateStrategies == null || statsUpdateStrategies.Length == 0)
            {
                Debug.LogWarning($"{gameEntity.GetName} has no stats update strategies assigned");
                return;
            }
            _statsStrats = new IOnStatsChangeStrategy[statsUpdateStrategies.Length];
            for (int i = 0; i < statsUpdateStrategies.Length; i++)
            {
                _statsStrats[i] = statsUpdateStrategies[i].BuildStrategy(this);
            }

        }

        private void Update()
        {
            if (Paused) return;

            if (currentAction != null)
            {
                switch (currentAction.UpdateAction(Time.deltaTime))
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
            if (_lockAction ||  Paused || !CanAct()) return;

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
            if (Paused || act == null) return;
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

        #endregion

        #region ipausable

        public bool Paused { get; set; } = false;

        #endregion

        #region on stat change

        public void HandleStatsUpdate(IDictionary<BaseStatType, StatValueContainer> stats)
        {
            if (_statsStrats == null || _statsStrats.Length == 0)
            {
                return;
            }
            foreach (var st in _statsStrats)
            {
                st.HandleStats(stats);
            }
        }


        #endregion

        #region IKillable


        public virtual void Kill()
        {
            Debug.Log($"{GetMainEntity.GetName} died");
            Paused = true;
            _deathAction?.StartAction();
        }
        #endregion
    }
}