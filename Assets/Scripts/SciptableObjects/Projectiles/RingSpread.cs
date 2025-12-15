using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Items.Projectiles
{
    public sealed class RingSpread : ISpreadStrategy
    {
        public IEnumerable<Quaternion> GetRotations(Quaternion baseRotation, ShootingConfig cfg)
        {
            int n = Mathf.Max(1, cfg.ProjectilesPerBurst);
            if (n == 1)
            {
                yield return baseRotation;
            }

            float half = cfg.ConeAngle * 0.5f; // degrees

            for (int i = 0; i < n; i++)
            {
                float yaw = 360f * (i / (float)n);
                // Tilt outward by 'half' around the local X axis, then spin around Z/Y
                var rot = baseRotation
                          * Quaternion.Euler(half, 0f, 0f)  // tip away from forward by half-angle
                          * Quaternion.AngleAxis(yaw, Vector3.forward); // ring rotation
                yield return rot;
            }
        }
    }
}