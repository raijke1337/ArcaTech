using Arcatech.Units.Control;
using UnityEngine;

namespace Arcatech.Actions
{
    [CreateAssetMenu(fileName = "actionResult_impulse", menuName = "Usables/Extra/MovementDirection Impulse")]
    public class MovementDirectionImpulseApplicationSerialized : SerializedActionResult
    {
        [Header("Impulse relative to USER movement direction")]
        [Range (-1,1)]public float relativeImpulseDirection;

        [Range(0, 10)] public float relativeImpulseMult = 1f;
        public override ActionResult Deserialize()
        {
            return new MovementDirectionImpulseResult(relativeImpulseDirection, relativeImpulseMult);
        }
    }


    public class MovementDirectionImpulseResult : ActionResult
    {
        float direction;
        private float mult;
        private IMove mover;
        private bool init = false;
        public MovementDirectionImpulseResult(float d, float m)
        {
             direction = d;
             mult = m;
        }
        public override bool ProduceResult(
            BaseGameEntityComponent user, BaseGameEntityComponent target,
            Vector3 place, Quaternion placeRot)
        {
            if (mover == null)
                target.TryGetComponent(out mover);   

            if (mover == null)
            {
                Debug.LogWarning($"[Impulse] IMove not found on {target?.name}");
                return false;
            }

            mover.ApplyImpulse(direction * mult);
            return true;
        }
    }
}