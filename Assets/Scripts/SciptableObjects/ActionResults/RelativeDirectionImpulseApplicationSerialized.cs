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
    
        public override ActionResult Deserialize()
        {
            return new RelativeDirectionImpulseResult(relativeImpulseDirection, relativeImpulseMult);
        }
    }

    public class RelativeDirectionImpulseResult : ActionResult
    {
        private readonly float _direction;
        private readonly float _mult;
        private IMove _mover;
        private bool _initialized;
    
        public RelativeDirectionImpulseResult(float d, float m)
        {
            _direction = d;
            _mult = m;
        }
    
        public override bool ProduceResult(BaseGameEntityComponent user, BaseGameEntityComponent target, Vector3 place, Quaternion placeRot)
        {
            // Инициализация
            if (!_initialized)
            {
                _initialized = target.TryGetComponent(out _mover);
            }
        
            if (_mover == null)
            {
                return false;
            }

            // Вычисляем направление от user к target
            Vector3 directionVector = (target.transform.position - user.transform.position).normalized;
        
            // Применяем множитель направления (1 = от user, -1 = к user)
            Vector3 finalImpulse = directionVector * _direction * _mult;
        
            _mover.ApplyImpulse(finalImpulse);
            return true;
        }
    }
}