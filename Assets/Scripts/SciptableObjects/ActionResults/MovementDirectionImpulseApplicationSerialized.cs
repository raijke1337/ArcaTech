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

        [Tooltip("Скорость рывка (м/с) при mult = 1. Раньше была зашита в ImpulseSpeed каждого исполнителя (8).")]
        public float baseSpeed = 8f;

        [Tooltip("0 = длительность по умолчанию у исполнителя")]
        public float duration = 1f;

        public override ActionResult Deserialize()
        {
            return new MovementDirectionImpulseResult(relativeImpulseDirection, relativeImpulseMult * baseSpeed, duration);
        }
    }


    public class MovementDirectionImpulseResult : ActionResult
    {
        private readonly float direction;
        private readonly float speed;
        private readonly float duration;

        public MovementDirectionImpulseResult(float d, float speed, float duration)
        {
            direction = d;
            this.speed = speed;
            this.duration = duration;
        }

        public override bool ProduceResult(
            BaseGameEntityComponent user, BaseGameEntityComponent target,
            Vector3 place, Quaternion placeRot)
        {
            if (target == null || !target.TryGetComponent(out IMove mover)) return false;

            // Рывок по текущему направлению движения; если юнит стоит - по направлению взгляда
            Vector3 dir = mover.MovementVector;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) dir = target.transform.forward;
            dir.Normalize();

            mover.ApplyMotion(MotionRequest.Dash(dir * (direction * speed), duration));
            return true;
        }
    }
}