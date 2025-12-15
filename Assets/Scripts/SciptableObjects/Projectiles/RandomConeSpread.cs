using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Items.Projectiles
{
    public sealed class RandomConeSpread : ISpreadStrategy
    {
        public IEnumerable<Quaternion> GetRotations(Quaternion baseRotation, ShootingConfig cfg)
        {
            int n = Mathf.Max(1, cfg.ProjectilesPerBurst);
            if (n == 1)
            {
                yield return baseRotation;
                yield break;
            }

            float halfRad = 0.5f * cfg.ConeAngle * Mathf.Deg2Rad;
            float cosHalf = Mathf.Cos(halfRad);

            for (int i = 0; i < n; i++)
            {
                float u = UnityEngine.Random.value;    // [0,1)
                float v = UnityEngine.Random.value;    // [0,1)

                float cosTheta = Mathf.Lerp(1f, cosHalf, u);
                float sinTheta = Mathf.Sqrt(1f - cosTheta * cosTheta);
                float phi = 2f * Mathf.PI * v;

                Vector3 localDir = new Vector3(Mathf.Cos(phi) * sinTheta, Mathf.Sin(phi) * sinTheta, cosTheta);

                Vector3 baseFwd = baseRotation * Vector3.forward;

                Quaternion rot = Quaternion.LookRotation(Quaternion.FromToRotation(Vector3.forward, baseFwd) * localDir, Vector3.up);
                if (cfg.RandomizeYawOnly)
                {
                    // If you need yaw-only, override pitch to 0 relative to base forward
                    Vector3 flatDir = Vector3.ProjectOnPlane(rot * Vector3.forward, Vector3.up).normalized;
                    rot = Quaternion.LookRotation(flatDir, Vector3.up);
                }

                yield return rot;
            }
        }
    }
}