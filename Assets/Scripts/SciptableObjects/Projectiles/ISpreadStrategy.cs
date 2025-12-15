using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Items.Projectiles
{
    public interface ISpreadStrategy
    {
        IEnumerable<Quaternion> GetRotations(Quaternion baseRotation, ShootingConfig cfg);
    }
}