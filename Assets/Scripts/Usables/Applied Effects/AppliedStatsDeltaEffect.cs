using System.Collections.Generic;
using Arcatech.Stats;
using UnityEngine;

namespace Arcatech.Usables.Effects
{
    [CreateAssetMenu(fileName = "usableEffect_stats_", menuName = "Usable/Applied Effects/Stats delta")]
    public class AppliedStatsDeltaEffect : BaseAppliedEffect
    {
        [Header("Instant delta")]
        public List<StatDelta> instantDeltas = new();

        [Header("Persistent Modifiers (active for effect lifetime)")]
        public List<StatModifier> persistentModifiers = new();

        [Header("Periodic Deltas")]
        public List<PeriodicDelta> periodicDeltas = new();
    }
}