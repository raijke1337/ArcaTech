using UnityEngine;

namespace Arcatech.Units.Control
{
    public interface IPlayerMovementController : IAim, IMove, IJump
    {
        public bool CanMove { get; set;}
        public bool CanAim { get; set; }
    }
    public interface IAim
    {
        public Vector3 AimPosition { get; set; }
    }

    public interface IMove
    {
        public bool CanMove { get; set; }
        public Vector3 MovementVector { get; set; }
    }

    public interface IJump
    {
        public bool JumpCommand { get; set; }
    }
}