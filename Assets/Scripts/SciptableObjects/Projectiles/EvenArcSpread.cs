using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Items.Projectiles
{
    public sealed class EvenArcSpread : ISpreadStrategy
    {
        public IEnumerable<Quaternion> GetRotations(Quaternion baseRotation, ShootingConfig cfg)
        {
            int n = Mathf.Max(1, cfg.ProjectilesPerBurst);
            if (n == 1)
            {
                yield return baseRotation;
                yield break;
            }

            float half = cfg.ConeAngle * 0.5f;
            for (int i = 0; i < n; i++)
            {
                float t = (n == 1) ? 0f : (i / (n - 1f));     // [0..1]
                float yaw = Mathf.Lerp(-half, half, t);       // degrees
                yield return baseRotation * Quaternion.Euler(0f, yaw, 0f);
            }
        }
    }
}