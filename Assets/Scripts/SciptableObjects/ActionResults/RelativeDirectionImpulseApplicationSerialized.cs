using Arcatech.Units.Control;
using UnityEngine;

namespace Arcatech.Actions
{
    [CreateAssetMenu(fileName = "actionResult_impulseRelative", menuName = "Usables/Extra/Relative Impulse")]
    public class RelativeDirectionImpulseApplicationSerialized : SerializedActionResult
    {        
        [Header("Impulse Direction")]
        [Range(-1, 1)]
        public float relativeImpulseDirection; 
        // 1 = target moves AWAY from user
        // -1 = target moves TOWARDS user
    
        [Range(0, 10)] 
        public float relativeImpulseMult = 1f;

        [Tooltip("Скорость (м/с) при mult = 1. Оставьте 1, чтобы поведение старых ассетов не изменилось.")]
        public float baseSpeed = 1f;

        [Tooltip("0 = длительность по умолчанию у цели")]
        public float duration = 0f;

        [Tooltip("Может ли цель (NPC) вылететь за край навмеша, например в пропасть")]
        public bool canLeaveNavMesh = true;
    
        public override ActionResult Deserialize()
        {
            return new RelativeDirectionImpulseResult(relativeImpulseDirection, relativeImpulseMult * baseSpeed, duration, canLeaveNavMesh);
        }
    }

    public class RelativeDirectionImpulseResult : ActionResult
    {
        private readonly float _direction;
        private readonly float _speed;
        private readonly float _duration;
        private readonly bool _canLeaveNavMesh;

        public RelativeDirectionImpulseResult(float d, float speed, float duration, bool canLeaveNavMesh)
        {
            _direction = d;
            _speed = speed;
            _duration = duration;
            _canLeaveNavMesh = canLeaveNavMesh;
        }
    
        public override bool ProduceResult(BaseGameEntityComponent user, BaseGameEntityComponent target, Vector3 place, Quaternion placeRot)
        {
            if (user == null || target == null) return false;
            if (!target.TryGetComponent(out IMove mover)) return false;

            // Направление от user к target по горизонтали (вертикаль - отдельная механика подброса)
            Vector3 away = target.transform.position - user.transform.position;
            away.y = 0f;
            if (away.sqrMagnitude < 1e-4f) away = user.transform.forward;   // стоят в одной точке - толкаем по взгляду
            away.Normalize();

            // _direction: 1 = от user, -1 = к user
            mover.ApplyMotion(MotionRequest.Knockback(away * (_direction * _speed), _duration, _canLeaveNavMesh));
            return true;
        }
    }
}