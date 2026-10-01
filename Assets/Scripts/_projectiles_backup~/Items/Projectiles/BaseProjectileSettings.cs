using System;
using UnityEngine;

namespace Arcatech.Items.Projectiles
{
    [Serializable]
    public struct BaseProjectileSettings
    {
        [Min(0.1f)]
        public float maxFlightDistance;
        [Min(0)]
        public float baseSpeed; 
        public AnimationCurve speedCurve;
        public float MaxFlightTime => maxFlightDistance/baseSpeed;
    }
}