using UnityEngine;

namespace Arcatech.Units.Control
{
    public interface IAim
    {
        bool CanAim { get; set; }
        Vector3 AimDirection { get; set; }
        public bool HasLockedTarget { get; set; }
    }
}