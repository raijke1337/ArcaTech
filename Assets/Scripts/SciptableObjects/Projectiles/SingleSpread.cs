using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Items.Projectiles
{
    public sealed class SingleSpread : ISpreadStrategy
    {
        public IEnumerable<Quaternion> GetRotations(Quaternion baseRotation, ShootingConfig cfg)
        {
            yield return baseRotation;
        }
    }
}