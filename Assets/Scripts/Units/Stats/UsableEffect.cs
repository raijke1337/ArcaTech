using System.Collections.Generic;
using Arcatech.Texts;
using UnityEngine;

namespace Arcatech.Stats
{
    /// <summary>
    /// todo add other types of effects (slow, buff..)
    /// </summary>
    [CreateAssetMenu(fileName = "StatsEffect", menuName = "Base Stats/Effect")]
    public class UsableEffect : ScriptableObject
    {
        [Header("Meta")]
        public Description description;

        [Header("Lifetime")]
        public bool infiniteDuration;
        [Tooltip("ignored if infinite")]public float durationSeconds = 3f; 

        [Header("Stacking")]
        public bool canStack = true;
        public int maxStacks = 99;

        [Header("Instant")]
        public List<StatDelta> instantDeltas = new();

        [Header("Persistent Modifiers (active for effect lifetime)")]
        public List<StatModifier> persistentModifiers = new();

        [Header("Periodic Deltas")]
        public List<PeriodicDelta> periodicDeltas = new();
    }
}