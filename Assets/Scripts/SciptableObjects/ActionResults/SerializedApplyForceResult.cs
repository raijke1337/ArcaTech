using Arcatech.Units;
using ECM.Components;
using UnityEngine;
using UnityEngine.Assertions;
using static Arcatech.Actions.SerializedApplyForceResult;

namespace Arcatech.Actions
{
    [CreateAssetMenu(fileName = "New apply force result ", menuName = "Actions/Action Result/ApplyForce", order = 2)]
    public class SerializedApplyForceResult : SerializedActionResult
    {
        public enum ForceTarget // bandaid
        {
            User,Target
        }

        [SerializeField, Tooltip("Negative for backwards movement")] float Distance;
        [SerializeField, Range (0,10)] float Speed;
        [SerializeField] ForceTarget Target;

        private void OnValidate()
        {
            Assert.IsFalse(Distance == 0);
        }
        public override IActionResult BuildActionResult()
        {
            return new ApplyForceResult(Distance, Speed, Target );
        }
    }

    public class ApplyForceResult : ActionResult
    {
        private float _d;
        private float _s;
        private ForceTarget _tgt;
        public ApplyForceResult (float distance, float speed, ForceTarget t)
        {
            _d = distance; _s = speed; _tgt = t;
        }
        public override void ProduceResult(BaseEntity user, BaseEntity target, Transform place)
        {
            switch (_tgt)
            {
                case ForceTarget.User:
                    user.ApplyForceResultToUnit(_s, _d);
                    break;
                case ForceTarget.Target:
                    target.ApplyForceResultToUnit(_s, _d);
                    break;
            }
        }
    }


}