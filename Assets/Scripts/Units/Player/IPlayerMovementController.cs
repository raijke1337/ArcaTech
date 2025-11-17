using UnityEngine;

namespace Arcatech.Units.Control
{
    public interface IPlayerMovementController : IAim, IMove, IJump
    {
 
    }
    public interface IAim
    {
        public bool CanAim { get; set; }
        public Vector3 AimPosition { get; set; }
    }

    public interface IMove
    {
        public bool CanMove { get; set; }
        public Vector3 MovementVector { get; set; }
        public bool IsGrounded { get; }
        public bool UseRootMotion { get; set; }
    }

    public interface IJump
    {
        public bool JumpCommand { get; set; }
    }
}