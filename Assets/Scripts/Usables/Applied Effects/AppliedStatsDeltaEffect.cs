using System.Collections.Generic;
using Arcatech.Texts;
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
    [CreateAssetMenu(fileName = "usableEffect_speed_", menuName = "Applied Effects/Speed change")]
    public class AppliedSpeedDeltaEffect : BaseAppliedEffect
    {
        [Range(0.01f, 1f)] public float percentSpeedMult = 0.2f;
    }
    [CreateAssetMenu(fileName = "usableEffect_damageTakenMod_", menuName = "Applied Effects/Damage taken mod")]
    public class AppliedDamageTakenModifierEffect : BaseAppliedEffect
    {
        [Range(0.01f, 1f)] public float percentDamageTakenMult = 0.2f;
    }
    [CreateAssetMenu(fileName = "usableEffect_damageDealtMod_", menuName = "Applied Effects/Damage dealt mod")]
    public class AppliedDamageDealtModifierEffect : BaseAppliedEffect
    {
        [Range(0.01f, 1f)] public float percentDamageMult = 0.2f;
    }
    [CreateAssetMenu(fileName = "usableEffect_Visual_", menuName = "Applied Effects/Visual overlay")]
    public class AppliedVisualEffect : BaseAppliedEffect
    {
        [SerializeField] public Material overlayMaterial;
    }

    public abstract class BaseAppliedEffect : ScriptableObject
    {
        [Header("Meta")]
        public Description description;

        [Header("Lifetime")]
        public bool infiniteDuration;
        [Tooltip("ignored if infinite")]public float durationSeconds = 3f; 

        [Header("Stacking")]
        public bool canStack = false;
        public int maxStacks = 99;
    }
}