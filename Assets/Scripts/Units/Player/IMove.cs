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
        /// <summary>Единая точка для отталкиваний и рывков. Направление и скорость считает вызывающий.</summary>
        public void ApplyMotion(in MotionRequest request);
        public bool IsGamepadInput { get; set; }
    }
}