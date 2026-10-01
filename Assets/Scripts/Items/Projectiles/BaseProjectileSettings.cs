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
        [Tooltip("Множитель скорости по нормализованному времени полёта (0..1). " +
                 "Время полёта вычисляется так, чтобы за него снаряд пролетел ровно maxFlightDistance.")]
        public AnimationCurve speedCurve;

        [Tooltip("Реакция на стены для поведений, которые не задают её сами (Basic, Homing).")]
        public EnvironmentResponse environmentResponse;

        /// <summary>
        /// Время полёта БЕЗ учёта кривой скорости (расстояние / скорость).
        /// Реальную длительность с кривой считает BaseProjectileBehavior.
        /// </summary>
        public float MaxFlightTime => maxFlightDistance / baseSpeed;
    }
}
