using Arcatech.Units;
using DG.Tweening;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.UIElements;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
namespace Arcatech
{
    /// <summary>
    /// new component to define a unit that actively does something in the game
    /// </summary>
    [RequireComponent (typeof(BaseGameEntityComponent),typeof(Animator))]
    public class ActiveGameUnitComponent : ValidatedMonoBehaviour
    {
        [SerializeField,Self] BaseGameEntityComponent gameEntity;
        [SerializeField, Self] protected Animator _animator;

        [Space, SerializeField] protected SerializedUnitAction ActionOnDamage;
        [SerializeField] protected SerializedUnitAction ActionOnDeath;
        [SerializeField, Tooltip("Place to spawn effects")] protected Transform _headT;
        public BaseGameEntityComponent GetMainEntity { get => gameEntity; }
        public Animator GetAnimatorReference => _animator;


        #region locks
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
        protected virtual void OnActionLock(bool locking) { }
        #endregion

        private void Update()
        {
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
        }

        #region actions

        protected BaseUnitAction currentAction;
        public virtual void Command (UnitActionType obj)
        {
            // this execution is blocked by ActionLock bool
            BaseUnitAction a;
            if (_lockAction) return;
            //if (!_ground.isOnGround) return;
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

   

        public void ForceUnitAction(BaseUnitAction act)
        {
            if (gameEntity.Paused|| act == null) return;
            OnForceAction(act);
        }
        protected virtual void OnForceAction(BaseUnitAction act)
        {
            DoActionLogic(act);
        }
        protected void DoActionLogic(BaseUnitAction act)
        {
            if (currentAction != null && currentAction != act && currentAction.GetActionState != UnitActionState.Completed)
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