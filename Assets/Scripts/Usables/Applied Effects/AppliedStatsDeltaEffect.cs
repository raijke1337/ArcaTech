using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Stats
{
    [CreateAssetMenu(fileName = "usableEffect_stats_", menuName = "Applied Effects/Stats delta")]
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