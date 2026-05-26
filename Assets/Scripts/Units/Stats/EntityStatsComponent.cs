using System;
using System.Collections.Generic;
using System.Linq;
using Arcatech.Items;
using Arcatech.Units;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Stats
{
    /// <summary>
    /// new component to handle the current stats and their changes on any game entity
    /// also uses stat change strategies to affect the rest of components
    /// </summary>
    public class EntityStatsComponent : MonoBehaviour, IUnitInventoryView, IPausableComponent,
        IKillableComponent, IAppliedEffectsTakerComponent<AppliedStatsDeltaEffect>, IKillerComponent
    {
        
        [Header("Config")] [SerializeField] private BaseStatsConfig startingConfig;
        [SerializeField] private bool preserveCurrentRatioOnMaxChange = true;
        
        private readonly Dictionary<ResourceStatType, StatRuntime> stats = new();
        private readonly Dictionary<SourceKey, List<StatModifier>> liveEquipMaxModifiers = new();
        
        private bool init = false;
        private void Awake()
        {
            if (init) return;
            InitializeFromConfig();
            TryGetComponent(out _aug);
        }
        
        private struct SourceKey : IEquatable<SourceKey>
        {
            public readonly object source;
            public readonly int id;

            public SourceKey(object src, int id)
            {
                source = src;
                this.id = id;
            }

            public bool Equals(SourceKey other) => ReferenceEquals(source, other.source) && id == other.id;
            public override bool Equals(object obj) => obj is SourceKey other && Equals(other);
            public override int GetHashCode() => ((source?.GetHashCode() ?? 0) * 397) ^ id;
            public override string ToString() => $"{source?.GetType().Name ?? "null"}#{id}";
        }

        private int _nextId = 1;
        private int NextId() => _nextId++;

        // Store equipment-provided Max modifiers (we’ll aggregate each recalc)

        private class PeriodicRuntime
        {
            public SourceKey key;
            public PeriodicDelta spec;
            public float accumulator;
            public float? expireAt; // null => infinite (equipment) or infinite effect
            public BaseGameEntityComponent sourceRef;
            public int stacks = 1;
        }

        private readonly List<PeriodicRuntime> periodic = new();

        public class AppliedEffectInstance
        {
            public AppliedStatsDeltaEffect effect;
            public float? expireAt;
            public object sourceRef;
            public int stacks = 1;
            public List<StatModifier> persistentMaxMods = new();
            
            
            public BaseAppliedEffect Effect;
        }

        private readonly List<AppliedEffectInstance> activeEffects = new();

        // If true, re-aggregate Max every frame to reflect conditional Max modifiers
        private bool _hasConditionalMaxMods = false;

        public void InitializeFromConfig()
        {
            stats.Clear();

            if (startingConfig != null)
            {
                foreach (var rs in startingConfig.resources)
                {
                    var st = new StatRuntime
                    {
                        baseMax = Mathf.Max(0f, rs.baseMax),
                        minClamp = rs.minClampCurrent,
                        maxClamp = rs.maxClampCurrent
                    };
                    st.max = st.baseMax;
                    float startCurrent = rs.setStartCurrentAsPercentOfMax
                        ? Mathf.Clamp01(rs.startPercent) * st.max
                        : rs.startCurrent;
                    st.current = Mathf.Clamp(startCurrent, st.minClamp, Mathf.Max(st.maxClamp, st.max));
                    stats[rs.stat] = st;
                }
            }

            liveEquipMaxModifiers.Clear();
            periodic.Clear();
            activeEffects.Clear();
            _hasConditionalMaxMods = false;
            init = true;
        }

        #region inventory
        public event UnityAction ViewChangedInventory;

        public void RefreshView(UnitInventoryModel model)
        {
            if (!init) InitializeFromConfig();
            
            RemoveAllEquipmentContributions();

//            Debug.Log("Refresh view");
            if (model != null)
            {
                int itemIndex = 0;
                foreach (var provider in model.EnumerateProviders())
                {
//                    Debug.Log($"provider {provider}");
                    
                    var key = new SourceKey(provider, itemIndex++);
                    ApplyEquipmentProvider(provider, key);
                }
            }

            RecomputeConditionalFlags();
            RecalculateAllMaxAndClampCurrent();
        }
        
        
        private void ApplyEquipmentProvider(IEquipmentStatsProvider provider, SourceKey key)
        {
            // 1) Persistent Max modifiers (null-safe)
            var modsEnum = provider.GetPersistentModifiers();
            List<StatModifier> mods = modsEnum != null ? modsEnum.ToList() : new List<StatModifier>();
            List<StatModifier> maxMods = mods.Where(m => m.target == StatTarget.Max).ToList(); // never null

            if (maxMods.Count > 0)
            {
                liveEquipMaxModifiers[key] = maxMods;
                if (!_hasConditionalMaxMods && maxMods.Any(m => !m.condition.IsEmpty))
                    _hasConditionalMaxMods = true;
            }

            // 2) Periodic deltas (null-safe)
            var pdsEnum = provider.GetPeriodicDeltas();
            if (pdsEnum != null)
            {
                foreach (var p in pdsEnum)
                {
                    periodic.Add(new PeriodicRuntime
                    {
                        key = key,
                        spec = p,
                        accumulator = 0f,
                        expireAt = null,
                        sourceRef = provider.Source
                    });
                }
            }

            // 3) Immediate recompute so changes show in the inspector right away
            RecomputeConditionalFlags();
            RecalculateAllMaxAndClampCurrent();
        }

        private void RemoveAllEquipmentContributions()
        {
            liveEquipMaxModifiers.Clear();

            for (int i = periodic.Count - 1; i >= 0; --i)
            {
                if (!periodic[i].expireAt.HasValue && periodic[i].key.source is IEquipmentStatsProvider)
                    periodic.RemoveAt(i);
            }
        }

        #endregion
        
        #region apply
        public bool ApplyEffect(AppliedStatsDeltaEffect eff, BaseGameEntityComponent s)
        {
            if (eff == null) return false;

            var key = new SourceKey(eff, NextId());
            float now = Time.time;
            float? expire = eff.infiniteDuration ? (float?)null : now + Mathf.Max(0f, eff.durationSeconds);

            foreach (var d in eff.instantDeltas)
                ApplyDelta(d, s, key);

            // Store persistent Max modifiers (with potential conditions)
            var effectMods = new List<StatModifier>();
            foreach (var m in eff.persistentModifiers)
            {
                if (m.target == StatTarget.Max)
                {
                    effectMods.Add(m);
                    if (!_hasConditionalMaxMods && !m.condition.IsEmpty) _hasConditionalMaxMods = true;
                }
            }

            foreach (var p in eff.periodicDeltas)
            {
                periodic.Add(new PeriodicRuntime
                {
                    key = key,
                    spec = p,
                    accumulator = 0f,
                    expireAt = expire,
                    sourceRef = s,
                    stacks = 1
                });
            }

            activeEffects.Add(new AppliedEffectInstance
            {
                effect = eff,
                expireAt = expire,
                sourceRef = s,
                stacks = 1,
                persistentMaxMods = effectMods
            });

            RecomputeConditionalFlags();
            RecalculateAllMaxAndClampCurrent();
            return true;
        }

        #endregion
        private void Update()
        {
            if (_killed || Paused) return;
            
            float dt = Time.deltaTime;
            float now = Time.time;
            bool anyAppliedTicks = false;

            for (int i = periodic.Count - 1; i >= 0; --i)
            {
                var pr = periodic[i];

                if (pr.expireAt.HasValue && now >= pr.expireAt.Value)
                {
                    periodic.RemoveAt(i);
                    continue;
                }

                pr.accumulator += dt;
                if (pr.spec.intervalSeconds <= 0f) pr.spec.intervalSeconds = 0.0001f;

                int ticks = 0;
                while (pr.accumulator >= pr.spec.intervalSeconds && ticks < 20)
                {
                    pr.accumulator -= pr.spec.intervalSeconds;
                    ticks++;

                    if (EvaluateConditionGroup(pr.spec.condition))
                    {
                        for (int s = 0; s < pr.stacks; s++)
                        {
                            ApplyDelta(pr.spec.delta, pr.sourceRef, pr.key);
                            anyAppliedTicks = true;
                        }
                    }
                }

                periodic[i] = pr;
            }

            // Expire ended effects (remove their persistent max modifiers via re-aggregation)
            bool expiredAnyEffects = false;
            for (int i = activeEffects.Count - 1; i >= 0; --i)
            {
                var ae = activeEffects[i];
                if (ae.expireAt.HasValue && now >= ae.expireAt.Value)
                {
                    activeEffects.RemoveAt(i);
                    expiredAnyEffects = true;
                }
            }

            if (_hasConditionalMaxMods || expiredAnyEffects || anyAppliedTicks)
            {
                // Re-aggregate Max if conditional mods exist (or effects ended, or ticks may have changed conditions)
                RecalculateAllMaxAndClampCurrent();
            }

            CheckKillCondition();
        }


        private void RecomputeConditionalFlags()
        {
            _hasConditionalMaxMods =
                liveEquipMaxModifiers.Values.Any(list => list.Any(m => !m.condition.IsEmpty)) ||
                activeEffects.Any(ae => ae.persistentMaxMods.Any(m => !m.condition.IsEmpty));
        }

        // Re-aggregate all Max values from base + equipment + effects, evaluating conditions
        private void RecalculateAllMaxAndClampCurrent()
        {
            // Reset per-stat contributions
            foreach (var kv in stats)
            {
                kv.Value.equipAddMax = 0f;
                kv.Value.equipMultMax = 0f;
                kv.Value.effectAddMax = 0f;
                kv.Value.effectMultMax = 0f;
            }

            // Equipment contributions (Max only)
            foreach (var kv in liveEquipMaxModifiers)
            {
                foreach (var m in kv.Value)
                {
                    if (!EvaluateConditionGroup(m.condition)) continue;
                    var sr = EnsureStat(m.stat);
                    if (m.op == StatOpKind.Add) sr.equipAddMax += m.value;
                    else sr.equipMultMax += m.value;
                }
            }

            // Effect contributions (Max only)
            foreach (var ae in activeEffects)
            {
                foreach (var m in ae.persistentMaxMods)
                {
                    if (!EvaluateConditionGroup(m.condition)) continue;
                    var sr = EnsureStat(m.stat);
                    if (m.op == StatOpKind.Add) sr.effectAddMax += m.value;
                    else sr.effectMultMax += m.value;
                }
            }

            // Compute final Max and clamp Current
            foreach (var kv in stats)
            {
                var st = kv.Value;
                float oldMax = st.max;

                float mult = (1f + st.equipMultMax) * (1f + st.effectMultMax);
                st.max = Mathf.Max(0f, (st.baseMax + st.equipAddMax + st.effectAddMax) * mult);

                if (preserveCurrentRatioOnMaxChange && oldMax > 0f)
                {
                    float ratio = st.current / oldMax;
                    st.current = ratio * st.max;
                }

                float clampMax = st.maxClamp > 0f ? Mathf.Min(st.maxClamp, st.max) : st.max;
                SetCurrentInternal(kv.Key, Mathf.Clamp(st.current, st.minClamp, clampMax), null, null);
            }
        }

        private void ApplyDelta(StatDelta d, BaseGameEntityComponent source, SourceKey key)
        {
            var sr = EnsureStat(d.stat);

            if (d.target == StatTarget.Max)
            {
                // Treat as temporary additive effect to Max: add to effectAddMax and recompute.
                float before = sr.max;
                sr.effectAddMax += d.amount;
                RecalculateAllMaxAndClampCurrent();
            }
            else
            {
                float clampMax = sr.maxClamp > 0f ? Mathf.Min(sr.maxClamp, sr.max) : sr.max;
                float newCurrent = Mathf.Clamp(sr.current + d.amount, sr.minClamp, clampMax);
                float delta = newCurrent - sr.current;
                if (Mathf.Abs(delta) > 0.0001f)
                    SetCurrentInternal(d.stat, newCurrent, source, key.source);
            }
        }

        private void SetCurrentInternal(ResourceStatType stat, float newCurrent, object source,
            object contributionSource)
        {
            var sr = EnsureStat(stat);
            float delta = newCurrent - sr.current;  
            if (Mathf.Abs(delta) <= 0.000001f) return;
            sr.current = newCurrent;
            UpdateViewers(stat, sr.current, sr.max, delta, contributionSource);
        }

        private StatRuntime EnsureStat(ResourceStatType stat)
        {
            if (!stats.TryGetValue(stat, out var sr))
            {
                sr = new StatRuntime
                {
                    baseMax = 0f, current = 0f, max = 0f,
                    minClamp = 0f, maxClamp = 0f,
                    equipAddMax = 0f, equipMultMax = 0f, effectAddMax = 0f, effectMultMax = 0f
                };
                stats[stat] = sr;
            }

            return sr;
        }


        #region Public

        bool HasStat(ResourceStatType stat) => stats.ContainsKey(stat);

        public bool TryGetCurrent(ResourceStatType stat, out float value)
        {
            value = 0f;
            if (HasStat(stat)) value = stats[stat].current;
            return HasStat(stat);
        }
        public float GetMax(ResourceStatType stat) => stats.TryGetValue(stat, out var sr) ? sr.max : 0f;
        public float GetBaseMax(ResourceStatType stat) => stats.TryGetValue(stat, out var sr) ? sr.baseMax : 0f;

        public bool CanApplyCost(AppliedStatsDeltaEffect cost)
        {
            if (cost == null) return true;
            if (cost.instantDeltas == null || cost.instantDeltas.Count == 0) return true;

            // Aggregate total negative Current deltas per stat
            var neededByStat = new Dictionary<ResourceStatType, float>();
            foreach (var d in cost.instantDeltas)
            {
                if (d.target != StatTarget.Current) continue;
                if (d.amount >= 0f) continue; // only costs (negative)
                if (neededByStat.TryGetValue(d.stat, out var sum))
                    neededByStat[d.stat] = sum + d.amount; // sum remains negative
                else
                    neededByStat[d.stat] = d.amount;
            }

            if (neededByStat.Count == 0) return true; // no actual cost

            // Validate affordability against current values and clamps
            foreach (var kvp in neededByStat)
            {
                var stat = kvp.Key;
                float totalNegative = kvp.Value; // negative value
                float required = -totalNegative; // positive amount required

                // Must have the stat
                if (!stats.TryGetValue(stat, out var sr)) return false;

                // Available buffer above minClamp
                float available = Mathf.Max(0f, sr.current - sr.minClamp);

                if (required > available)
                    return false;
            }

            return true;
        }

        #endregion

        #region IStatUpdatesViewer

        private List<IStatUpdatesViewer> statUpdatesViewers = new();

        public void RegisterStatsViewer(IStatUpdatesViewer viewer)
        {
            if (statUpdatesViewers.Contains(viewer)) return;
            statUpdatesViewers.Add(viewer);
            StartViewer(viewer);
        }

        private void StartViewer(IStatUpdatesViewer viewer)
        {
            foreach (var stat in stats)
            {
                viewer.HandleStatsUpdate(stat.Key,stat.Value.current,stat.Value.max,0,null);
            }
        }
        private void UpdateViewers(ResourceStatType type, float current, float max, float delta,
            object contributionSource)
        {
            foreach (var v in statUpdatesViewers)
            {
                v.HandleStatsUpdate(type, current, max, delta, contributionSource);
            }
        }

        #endregion


        // Condition evaluation
        private bool EvaluateConditionGroup(ConditionGroup group)
        {
            if (group.IsEmpty) return true;
            if (!init) InitializeFromConfig();
            

            bool result = group.requireAll;
            foreach (var c in group.statConditions)
            {
                bool pass = EvaluateCondition(c);
                if (group.requireAll)
                {
                    if (!pass)
                    {
                        result = false;
                        break;
                    }
                }
                else
                {
                    if (pass)
                    {
                        result = true;
                        break;
                    }
                    else result = false;
                }
            }

            return group.invert ? !result : result;
        }

        private bool EvaluateCondition(StatCondition c)
        {
            // Get value to compare
            float val;
            if (c.target == StatTarget.Current)
            {
                float cur;
                TryGetCurrent(c.stat, out cur);
                if (c.usePercentOfMax)
                {
                    float max = Mathf.Max(0.00001f, GetMax(c.stat));
                    val = cur / max; // normalized 0..1
                }
                else val = cur;
            }
            else // Max
            {
                float max = GetMax(c.stat);
                if (c.usePercentOfMax)
                {
                    float d = Mathf.Max(0.00001f, max);
                    val = max / d; // ≈ 1; mostly not useful but defined
                }
                else val = max;
            }

            const float eps = 0.0001f;
            switch (c.op)
            {
                case ConditionOp.Greater: return val > c.a;
                case ConditionOp.GreaterOrEqual: return val >= c.a;
                case ConditionOp.Less: return val < c.a;
                case ConditionOp.LessOrEqual: return val <= c.a;
                case ConditionOp.Equal: return Mathf.Abs(val - c.a) <= eps;
                case ConditionOp.NotEqual: return Mathf.Abs(val - c.a) > eps;
                case ConditionOp.Between:
                    float min = Mathf.Min(c.a, c.b);
                    float maxv = Mathf.Max(c.a, c.b);
                    return val >= min - eps && val <= maxv + eps;
                default: return true;
            }
        }

        public bool Paused { get; set; }

        public void SetKilled(IKillerComponent c, bool value)
        {
            _killed = value;
            if (value)
            {
                InitializeFromConfig(); // reset unit stats, called on reload checkpoint
            }
        }

        private bool _killed;
        public bool CheckStatsConditionGroup(ConditionGroup group) =>  EvaluateConditionGroup(group);
        
        
        #region failsafe for no state augmentor kill condition

        private StatsStateAugmentorComponent _aug;
        private void CheckKillCondition()
        {
            if (_aug) return;
            if (stats[ResourceStatType.Health].current <= 0f)
            {
                var killables = GetComponentsInChildren<IKillableComponent>(true);
                foreach (var k in killables)
                {
                    k.SetKilled(this,true);
                }

                Debug.Log($"placeholder kill {name}");
                if (TryGetComponent<Animator>(out var animator)) animator.Play("Dead");
            }
        }
        public string KilledBy => "Stats 0 hp";
        
        #endregion
    }
}