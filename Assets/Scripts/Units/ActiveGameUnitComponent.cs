using Arcatech.Units;
using DG.Tweening;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.UIElements;
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
        public BaseGameEntityComponent GetMainEntity { get => gameEntity; }
        public Animator GetAnimatorReference => _animator;



        [Space, SerializeField] protected SerializedUnitAction ActionOnDamage;
        [SerializeField] protected SerializedUnitAction ActionOnDeath;
        [SerializeField, Tooltip("Place to spawn effects")] protected Transform _headT;        




        public void ForceUnitAction(BaseUnitAction act)
        {
            if (gameEntity.Paused|| act == null) return;
            OnForceAction(act);
        }
        protected virtual void OnForceAction(BaseUnitAction act)
        {
            act.StartAction();
        }

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
    }
}