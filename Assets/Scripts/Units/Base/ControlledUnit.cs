using Arcatech.Items;
using Arcatech.Managers;
using Arcatech.Triggers;
using Arcatech.Units.Stats;
using ECM.Components;
using KBCore.Refs;
using System.Collections;
using UnityEngine;
namespace Arcatech.Units
{
    [RequireComponent(typeof(ControlInputsBase), typeof(Rigidbody),typeof(GroundDetection))]
    public abstract class ControlledUnit : ArmedUnit, IInteractible
    {
        [Space, Header("Controlled Unit")]
         [SerializeField] protected MovementStatsConfig movementStats;
        [SerializeField, Self] protected Rigidbody _rb;
        [Self, SerializeField] protected ControlInputsBase _inputs;
        [Self, SerializeField] protected GroundDetection _ground;
        #region MANAGED
        public override void StartControllerUnit()
        {
            base.StartControllerUnit();
            if (GameManager.Instance.GetCurrentLevelData.LevelType == LevelType.Game)
            {
                UnitPaused = false;
                _inputs.StartController();
            }
            _inputs.UnitActionRequestedEvent += HandleUnitAction;
            _inputs.RequestInteraction += HandleInteractionAction;
            if (_stats.TryGetStatValue(BaseStatType.Stamina, out var stam))
            {
                stunEndStamina = Mathf.Clamp(stunEndStamina, stam.GetMin, stam.GetMax);
            }
            else
            {
                stunEndStamina = 0;
            }
           
        }
        public override void RunUpdate(float delta)
        {
            base.RunUpdate(delta);
            if (_stunned)
            {
                if (_stats.TryGetStatValue(BaseStatType.Stamina, out var s))
                {
                    if (s.GetCurrent >= stunEndStamina && stunEndProgress == null)
                    {
                        stunEndProgress = StartCoroutine(StunCancelCoroutine());
                    }
                }
                else return;
            }
            if (currentAction != null)
            {
                switch (currentAction?.UpdateAction(delta))
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
            _inputs.ControllerUpdate(delta);
        }
                public override void DisableUnit()
        {
            base.DisableUnit();
            _inputs.UnitActionRequestedEvent -= HandleUnitAction;
            _inputs.StopController();
        }
        #endregion

        [Space,Header("Stuns")]

        [SerializeField] protected SerializedUnitAction ActionOnStun;
        [SerializeField, Range(0, 300)] protected float stunStartStamina = 0f;
        [SerializeField, Range(0, 300)] protected float stunEndStamina = 30f;
        [SerializeField, Range(0.01f, 1)] protected float stunEndGetUpTime = 0.5f;

        Coroutine stunEndProgress;


        protected bool _stunned = false;
        public bool UnitStunned
        {
            get => _stunned;
            protected set
            {
                _stunned = value;

                if (_showDebugs) Debug.Log($"Entity stunned: {value}");
            }
        }

        IEnumerator StunCancelCoroutine()
        {
            yield return new WaitForSeconds(stunEndGetUpTime);
            _stunned = false;
            _animator.SetTrigger("StunEnd");
            ActionLock = false;
            stunEndProgress = null;
            yield return null;
        }
        protected override void StunAction()
        {
            if (UnitStunned) return;
            OnForceAction(ActionOnStun.ProduceAction(this,transform));
            UnitStunned = true;
        }

        protected override void OnTimedStatsUpdate()
        {
            if (_stats.TryGetStatValue(BaseStatType.Stamina, out var stam))
            {
                if (stam.GetCurrent <= stunStartStamina)
                {
                    StunAction();
                }
                
            }
            base.OnTimedStatsUpdate();
        }

        #region action lock


        bool _lockAction;
        protected bool ActionLock
        {
            get => _lockAction;
            set
            {
                OnActionLock(value);
                _lockAction = value;
            }
        }
        protected abstract void OnActionLock(bool locking);

#endregion

        #region base unit actions

        protected BaseUnitAction currentAction;

        protected override void OnForceAction(BaseUnitAction act)
        {
            base.OnForceAction(act);
            DoActionLogic(act);
        }

        protected void DoActionLogic(BaseUnitAction act)
        {
            if (currentAction!= null && currentAction != act && currentAction.GetActionState != UnitActionState.Completed)
            {
                currentAction.CompleteAction();
            }
            currentAction = act;
            ActionLock = currentAction.LockMovement;
            currentAction.StartAction();
        }
        public bool CanDoAction(UnitActionType action)
        {
            if (!_ground.isOnGround) return false;
            else return action switch
            {
                UnitActionType.Melee => _weapons.CanUseAction(action),
                UnitActionType.Ranged => _weapons.CanUseAction(action),
                UnitActionType.DodgeSkill => _skills.CanUseAction(action),
                UnitActionType.MeleeSkill => _skills.CanUseAction(action),
                UnitActionType.RangedSkill => _skills.CanUseAction(action),
                UnitActionType.ShieldSkill => _skills.CanUseAction(action),
                _ => false,
            };
        }
        protected virtual void HandleUnitAction(UnitActionType obj)
        {
            // this execution is blocked by ActionLock bool
            BaseUnitAction a;

            if (!_ground.isOnGround) return;
            switch (obj)
            {
                case UnitActionType.Melee:
                    if (_weapons.TryUseAction(obj, out a)) DoActionLogic(a);
                    break;
                case UnitActionType.Ranged:
                    if (_weapons.TryUseAction(obj, out a)) DoActionLogic(a);
                    break;
                case UnitActionType.DodgeSkill:
                    if (_skills.TryUseAction(obj, out a)) DoActionLogic(a);
                    break;
                case UnitActionType.MeleeSkill:
                    if (_skills.TryUseAction(obj, out a)) DoActionLogic(a);
                    break;
                case UnitActionType.RangedSkill:
                    if (_skills.TryUseAction(obj, out a)) DoActionLogic(a);
                    break;
                case UnitActionType.ShieldSkill:
                    if (_skills.TryUseAction(obj, out a)) DoActionLogic(a);
                    break;
                default:
                    Debug.LogWarning($"action type {obj} not supported in {this}");
                    break;
            }    
        }

        #endregion

        #region interaction

        public virtual void ReceiveInteraction(IInteractible interactible)
        {
            if (_showDebugs) Debug.Log($"NYI: {this} receives interaction from {interactible}");
        }

        protected abstract void HandleInteractionAction(IInteractible i);

        #endregion

    }
}