// EntityStatsComponent.DebugView.cs
// Partial class exposing read-only debug views for the custom inspector.

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Arcatech.Stats
{
    public partial class EntityStatsComponent
    {
        [Serializable]
        public struct DebugStatView
        {
            public ResourceStatType stat;
            public float baseMax;
            public float current;
            public float max;
            public float minClamp;
            public float maxClamp;
            public float equipAddMax;
            public float equipMultMax;
            public float effectAddMax;
            public float effectMultMax;

            public float Ratio => max > 0f ? current / max : 0f;
        }

        [Serializable]
        public struct DebugEffectView
        {
            public string displayName;
            public UsableEffect effectAsset;
            public int stacks;
            public float? secondsRemaining;
            public IReadOnlyList<StatModifier> persistentMaxMods;
            public UnityEngine.Object sourceRef;
        }

        [Serializable]
        public struct DebugPeriodicView
        {
            public string sourceLabel;
            public bool isEquipment;
            public PeriodicDelta spec;
            public float accumulator;
            public float? secondsRemaining;
            public int stacks;
        }

        [Serializable]
        public struct DebugEquipmentSourceView
        {
            public string sourceLabel;
            public IReadOnlyList<StatModifier> maxModifiers;
        }

        public DebugStatView[] GetDebugStats()
        {
            var arr = new List<DebugStatView>(stats.Count);
            foreach (var kv in stats)
            {
                var st = kv.Value;
                arr.Add(new DebugStatView
                {
                    stat = kv.Key,
                    baseMax = st.baseMax,
                    current = st.current,
                    max = st.max,
                    minClamp = st.minClamp,
                    maxClamp = st.maxClamp,
                    equipAddMax = st.equipAddMax,
                    equipMultMax = st.equipMultMax,
                    effectAddMax = st.effectAddMax,
                    effectMultMax = st.effectMultMax
                });
            }

            // Stable order for display
            arr.Sort((a, b) => a.stat.CompareTo(b.stat));
            return arr.ToArray();
        }

        public DebugEffectView[] GetDebugEffects()
        {
            float now = Application.isPlaying ? Time.time : 0f;

            var arr = new DebugEffectView[activeEffects.Count];
            for (int i = 0; i < activeEffects.Count; i++)
            {
                var ae = activeEffects[i];
                float? rem = ae.expireAt.HasValue ? Mathf.Max(0f, ae.expireAt.Value - now) : (float?)null;
                arr[i] = new DebugEffectView
                {
                    displayName = ae.effect != null ? ae.effect.description.Title : "(unnamed effect)",
                    effectAsset = ae.effect,
                    stacks = ae.stacks,
                    secondsRemaining = rem,
                    persistentMaxMods = ae.persistentMaxMods,
                    sourceRef = ae.sourceRef as UnityEngine.Object
                };
            }

            return arr;
        }

        public DebugPeriodicView[] GetDebugPeriodic()
        {
            float now = Application.isPlaying ? Time.time : 0f;

            var arr = new DebugPeriodicView[periodic.Count];
            for (int i = 0; i < periodic.Count; i++)
            {
                var pr = periodic[i];
                bool isEquip = pr.expireAt.HasValue == false && pr.key.source is IEquipmentStatsProvider;
                float? rem = pr.expireAt.HasValue ? Mathf.Max(0f, pr.expireAt.Value - now) : (float?)null;

                string label = pr.key.source != null
                    ? $"{pr.key.source.GetType().Name}#{pr.key.id}"
                    : $"Source#{pr.key.id}";

                arr[i] = new DebugPeriodicView
                {
                    sourceLabel = label,
                    isEquipment = isEquip,
                    spec = pr.spec,
                    accumulator = pr.accumulator,
                    secondsRemaining = rem,
                    stacks = pr.stacks
                };
            }

            return arr;
        }

        public DebugEquipmentSourceView[] GetDebugEquipmentModifiers()
        {
            var list = new List<DebugEquipmentSourceView>(liveEquipMaxModifiers.Count);
            foreach (var kv in liveEquipMaxModifiers)
            {
                string label = kv.Key.source != null
                    ? $"{kv.Key.source.GetType().Name}#{kv.Key.id}"
                    : $"EquipSource#{kv.Key.id}";

                list.Add(new DebugEquipmentSourceView
                {
                    sourceLabel = label,
                    maxModifiers = kv.Value
                });
            }

            return list.ToArray();
        }

        public bool IsRuntimeInitializedForPreview()
        {
            // Simple heuristic: if we have any stats populated, assume initialized
            return stats.Count > 0;
        }
    }
}
#endif