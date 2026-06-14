using Arcatech.Stats;
using UnityEngine;

namespace Arcatech.Usables.Effects
{
    [CreateAssetMenu(fileName = "usableEffect_shield_", menuName = "Usable/Applied Effects/Shield")]
    public class AppliedAbsorbShieldEffect : BaseAppliedEffect
    {
        [Header("Absorb shield")]
        public ResourceStatType absorbedStat = ResourceStatType.Health;
        [Tooltip("Buffer value added per tick.")]
        public float absorbValuePerTick = 100f;
        [Range(0f, 1f)] public float absorbCoefficient = 0.6f;
        [Tooltip("Max value the buffer can accumulate within this effect.")]
        public float absorbLimit = 150f;
        [Tooltip("Lifetime of the buffer itself (separate from effect duration).")]
        public float bufferLifetimeSeconds = 5f;
    }
}