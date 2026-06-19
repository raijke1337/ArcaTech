using UnityEngine;

namespace Arcatech.Units.Control
{
    public interface IAim
    {
        public bool CanAim { get; set; }
        public Vector3 AimPosition { get; set; }
    }

    public interface IJump
    {
        public bool JumpCommand { get; set; }
    }
}