using UnityEngine;

namespace Arcatech.Units.Control
{
    public interface IMove
    {
        public bool CanMove { get; set; }
        public Vector3 MovementVector { get; set; }
        public float ActualMovementVelocity { get; }
        public bool IsGrounded { get; }
        public bool UseRootMotion { get; set; }
        public float SpeedMultiplier { get; set; }
        public void ApplyImpulse(Vector3 impulse);
        public void ApplyImpulse(float impulseRelative);
    }
}