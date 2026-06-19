using Arcatech.Units.Control;
using UnityEngine;

namespace Arcatech.Actions
{

    [CreateAssetMenu(fileName = "actionResult_impulse", menuName = "Usables/Extra/Relative Impulse (push-back)")]
    public class RelativeImpulseApplicationSerialized : SerializedActionResult
    {
        [Header("Impulse relative to USER movement direction")]
        [Range (-1,1)]public float relativeImpulseDirection;
        [Range(0,10)] public float relativeImpulseMult;
        public override ActionResult Deserialize()
        {
            return new RelativeImpulseResult(relativeImpulseDirection, relativeImpulseMult);
        }
    }


    public class RelativeImpulseResult : ActionResult
    {
        float direction;
        private float mult;
        private IMove mover;
        private bool init = false;
        public RelativeImpulseResult(float d, float m)
        {
         direction = d;
         mult = m;
        }
        public override bool ProduceResult(BaseGameEntityComponent user, BaseGameEntityComponent target, Vector3 place, Quaternion placeRot)
        {
            if (!init)
            {
                if (user.TryGetComponent(out mover));
            }

            if (mover == null) return false;
            mover.ApplyImpulse(direction);
            return true;
        }
    }
}