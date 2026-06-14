using Arcatech.Texts;
using UnityEngine;

namespace Arcatech.Usables.Effects
{
    public abstract class BaseAppliedEffect : ScriptableObjectID
    {
        
        private void OnValidate()
        {
            // Old assets serialized before "periodicity" existed get enum default (0).
            // PeriodicityKind default(0) == OneShot, but ticks default 0 -> guard it.
            if (periodicity.ticks < 1) periodicity.ticks = 1;

            // Heuristic migration: if this effect has only instant deltas and no explicit
            // repeating setup, force OneShot/AtStart so it behaves like the old instant path.
            // (Safe: designers can still switch to Repeating manually.)
        }
        public enum StackType { None, Refresh, Independent }

        [Header("Meta")] public Description description;

        [Header("Lifetime")]
        public bool infiniteDuration;
        [Tooltip("ignored if infinite")] public float durationSeconds = 3f;

        [Header("Stacking")]
        public StackType stackType;
        public int maxStacks = 99;

        // NEW: common timing for every effect kind (design doc section "Б) Периодичностью")
        [Header("Periodicity")]
        public PeriodicityDefinition periodicity = new PeriodicityDefinition
        {
            kind = PeriodicityKind.OneShot,
            oneShotMoment = OneShotMoment.AtStart,
            ticks = 1,
            intervalMode = IntervalMode.After,
            offsetSeconds = 0f
        };
    }
}