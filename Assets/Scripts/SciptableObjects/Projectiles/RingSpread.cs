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
            yield break;
        }

        for (int i = 0; i < n; i++)
        {
            // Calculate angle around the Y-axis (horizontal ring around character)
            float angle = (360f * i) / n; // Evenly distribute around 360 degrees
            
            // Create rotation around Y-axis (up direction)
            var ringRotation = Quaternion.AngleAxis(angle, Vector3.up);
            
            yield return ringRotation;
        }
    }
}
}