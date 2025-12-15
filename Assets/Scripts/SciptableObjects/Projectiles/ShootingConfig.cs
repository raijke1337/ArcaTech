using System;
using UnityEngine;

namespace Arcatech.Items.Projectiles
{
    public enum PatternType
    {
        Single,
        Arc,
        Ring,
        Cone
    }

    [Serializable]
    public struct ShootingConfig
    {
        public SpawningPlaceType placeType;
       [Min(1)] public int TotalBursts;                  // burst groups
       [Min(0)] public float BetweenBurstsDelay;    // delay between groups

        public PatternType Pattern;
        [Min(1)] public int ProjectilesPerBurst;         // how many projectiles simultaneously
        [Min(1)] public float ConeAngle;            // degrees (full angle)
        public bool RandomizeYawOnly;      // false = 3D cone, true = horizontal only
        [Min(0)] public float PelletSpawnRadius;    // optional small position spread
    }
}