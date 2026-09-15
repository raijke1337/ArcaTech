using System;
using System.Collections.Generic;
using System.Linq;
using Arcatech.Items;
using Arcatech.Units;
using Arcatech.Usables.Effects;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Stats
{
    /// <summary>
    /// Handles current stats and their changes on a game entity.
    /// Aggregates Max from base + equipment + effects, ticks periodic deltas,
    /// intercepts damage through shields and triggers the kill condition.
    /// </summary>
    public class EntityStatsComponent : ValidatedMonoBehaviour, IUnitInventoryView, IPausableComponent,
        IKillableComponent, IStatReceiver, IShieldReceiver, IKillerComponent
    {
        #region Nested types

        public enum ExpendType
        {
            None,
            UsableCost,
            ActionResult,
            Equipment
        }

        private readonly struct SourceKey : IEquatable<SourceKey>
        {
            public readonly BaseGameEntityComponent source;
            public readonly int id;
            public readonly ExpendType expendType;

            public SourceKey(BaseGameEntityComponent src, int id, ExpendType type)
            {
                source = src;
                this.id = id;
                expendType = type;
            }

            public bool Equals(SourceKey other) =>
                ReferenceEquals(source, other.source) && id == other.id && expendType == other.expendType;

            public override bool Equals(object obj) => obj is SourceKey other && Equals(other);

            // FIX: expendType теперь участвует в хэше (согласовано с Equals)
            public override int GetHashCode() =>
                (((source?.GetHashCode() ?? 0) * 397) ^ id) * 31 + (int)expendType;

            public override string ToString() => $"{source?.GetType().Name ?? "null"}#{id}:{expendType}";
        }

        private class PeriodicRuntime
        {
            public SourceKey key;
            public PeriodicDelta spec;
            public float accumulator;
            public float? expireAt;            // null => бесконечно (экипировка / infinite effect)
            public BaseGameEntityComponent sourceRef;
            public int stacks = 1;
        }

        public class AppliedEffectInstance
        {
            public AppliedStatsDeltaEffect effect;
            public float? expireAt;
            public object sourceRef;
            public int stacks = 1;
            public List<StatModifier> persistentMaxMods = new();
            // FIX: удалено дублирующее неиспользуемое поле BaseAppliedEffect Effect
        }

        #endregion

        #region Inspector

        [Header("Config")]
        [SerializeField] private BaseStatsConfig startingConfig;
        [SerializeField] private bool preserveCurrentRatioOnMaxChange = true;
        [SerializeField, Self] private BaseGameEntityComponent entity;

        [Space, Header("Kill Condition")]
        [SerializeField] private bool useKillCondition = false;
        [Tooltip("Например: Health / Current / LessOrEqual / 0")]
        [SerializeField] private StatCondition killCondition;

        #endregion

        #region Runtime state

        private readonly Dictionary<ResourceStatType, StatRuntime> stats = new();
        private readonly Dictionary<SourceKey, List<StatModifier>> liveEquipMaxModifiers = new();
        private readonly List<PeriodicRuntime> periodic = new();
        private readonly List<AppliedEffectInstance> activeEffects = new();
        private readonly List<ShieldBuffer> _shields = new();
        private readonly List<IStatUpdatesViewer> statUpdatesViewers = new();
        private readonly Dictionary<ResourceStatType, float> _lastNotifiedShieldByStat = new();

        private bool init;
        private bool _killed;
        private bool _hasConditionalMaxMods;   // true => переагрегировать Max каждый кадр
        private int _nextId = 1;
        private IDamageDrawer _damageDrawer;
        private BaseGameEntityComponent _lastHarmSource; // для KilledBy

        private const int MaxTicksPerFrame = 20;
        private const float MinTickInterval = 0.0001f;

        #endregion

        #region Public properties

        public bool Paused { get; set; }
        public bool Invulnerable { get; set; }
        public string KilledBy => $"Stats Kill Condition (source: {_lastHarmSource?.name ?? "unknown"})";

        public event UnityAction ViewChangedInventory;

        #endregion

        #region Unity lifecycle

        private void Awake()
        {
            if (init) return;
            InitializeFromConfig();
        }

        private void OnEnable()
        {
            TryGetComponent(out _damageDrawer);
        }

        private void Update()
        {
            if (_killed || Paused) return;

            float dt = Time.deltaTime;
            float now = Time.time;

            bool anyAppliedTicks = TickPeriodic(dt, now);
            bool expiredAnyEffects = ExpireEffects(now);

            if (_hasConditionalMaxMods || expiredAnyEffects || anyAppliedTicks)
                RecalculateAllMaxAndClampCurrent();

            TickShields(dt);
        }

        #endregion

        #region Initialization

        public void InitializeFromConfig()
        {
            stats.Clear();

            if (startingConfig != null)
            {
                foreach (var rs in startingConfig.resources)
                {
                    var st = new StatRuntime
                    {
                        baseMax = Mathf.Max(0f, rs.Value.baseMax),
                        minClamp = rs.Value.minClampCurrent,
                        maxClamp = rs.Value.maxClampCurrent
                    };
                    st.max = st.baseMax;
                    float startCurrent = rs.Value.setStartCurrentAsPercentOfMax
                        ? Mathf.Clamp01(rs.Value.startPercent) * st.max
                        : rs.Value.startCurrent;
                    st.current = Mathf.Clamp(startCurrent, st.minClamp, Mathf.Max(st.maxClamp, st.max));
                    stats[rs.Key] = st;
                }
            }
            else
            {
                Debug.LogWarning($"No stats assigned for {entity}! Creating default 100 hp.");
                stats[ResourceStatType.Health] = new StatRuntime
                {
                    baseMax = 100,
                    minClamp = 0,
                    maxClamp = 100,
                    max = 100,
                    current = 100 // FIX: раньше current оставался 0 => мгновенная смерть с kill condition
                };
            }

            liveEquipMaxModifiers.Clear();
            periodic.Clear();
            activeEffects.Clear();
            _shields.Clear();                    // FIX: щиты переживали перезагрузку чекпоинта
            _lastNotifiedShieldByStat.Clear();
            _hasConditionalMaxMods = false;
            _lastHarmSource = null;
            init = true;
        }

        #endregion

        #region Kill condition

        public void SetKilled(IKillerComponent c, bool value)
        {
            _killed = value;
            if (!value)
            {
                InitializeFromConfig(); // reset unit stats, called on reload checkpoint
            }
        }

        /// <summary>
        /// Вызывается после каждого фактического изменения Current.
        /// </summary>
        private void CheckKillCondition(BaseGameEntityComponent lastSource)
        {
            if (_killed || !useKillCondition) return;
            if (!EvaluateCondition(killCondition)) return;

            _killed = true;
            _lastHarmSource = lastSource != null ? lastSource : _lastHarmSource;
            OnKillCondition();
        }

        private void OnKillCondition()
        {
            entity.SetKilled(this, true);
        }

        #endregion

        #region Inventory (IUnitInventoryView)

        public void RefreshView(InventoryChangeNotification notification)
        {
            if (notification.ChangeType == InventoryChangeType.PickUp ||
                notification.ChangeType == InventoryChangeType.Use) return;

            if (!init) InitializeFromConfig();

            RemoveAllEquipmentContributions();

            var model = notification.InventorySnapshot;
            if (model != null)
            {
                int itemIndex = 0;
                foreach (var provider in model.EnumerateProviders())
                {
                    var key = new SourceKey(entity, itemIndex++, ExpendType.Equipment);
                    ApplyEquipmentProvider(provider, key);
                }
            }

            // Один пересчёт после всех провайдеров (раньше — на каждый предмет)
            RecomputeConditionalFlags();
            RecalculateAllMaxAndClampCurrent();
        }

        private void ApplyEquipmentProvider(IEquipmentStatsProvider provider, SourceKey key)
        {
            // 1) Persistent Max modifiers (null-safe)
            var mods = provider.GetPersistentModifiers()?.ToList() ?? new List<StatModifier>();
            var maxMods = mods.Where(m => m.target == StatTarget.Max).ToList();
            if (maxMods.Count > 0)
                liveEquipMaxModifiers[key] = maxMods;

            // 2) Periodic deltas (null-safe)
            var pds = provider.GetPeriodicDeltas();
            if (pds == null) return;

            foreach (var p in pds)
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
            // FIX: пересчёт убран отсюда — делается один раз в RefreshView
        }

        private void RemoveAllEquipmentContributions()
        {
            liveEquipMaxModifiers.Clear();

            // FIX: раньше проверялось "key.source is IEquipmentStatsProvider", что не срабатывало
            // (в key.source лежит entity) и периодика экипировки дублировалась при каждом RefreshView.
            periodic.RemoveAll(p => !p.expireAt.HasValue && p.key.expendType == ExpendType.Equipment);
        }

        #endregion

        #region Applying deltas & effects

        public bool ApplyUsableCost(AppliedStatsDeltaEffect eff, BaseGameEntityComponent s)
        {
            if (eff == null) return false;

            var key = new SourceKey(s, NextId(), ExpendType.UsableCost);
            float now = Time.time;
            float? expire = eff.infiniteDuration ? (float?)null : now + Mathf.Max(0f, eff.durationSeconds);

            foreach (var d in eff.instantDeltas)
                ApplyDelta(d, s, key);

            // Persistent Max modifiers (с возможными условиями)
            var effectMods = eff.persistentModifiers
                .Where(m => m.target == StatTarget.Max)
                .ToList();

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

        public bool ApplyInstantDelta(StatDelta delta, BaseGameEntityComponent source, EffectKey key)
        {
            bool isHarm = delta.target == StatTarget.Current && delta.amount < 0f;

            // FIX: неуязвимость блокирует только урон, а не лечение/оплату
            if (Invulnerable && isHarm) return false;

            // Shield interception: только входящий урон
            if (isHarm && _shields.Count > 0)
            {
                float damage = AbsorbThroughShields(delta.stat, -delta.amount);
                delta.amount = -damage;
            }

            var localKey = new SourceKey(source, key.SourceId?.GetHashCode() ?? 0, ExpendType.ActionResult);
            ApplyDelta(delta, source, localKey);
            return true;
        }

        private void ApplyDelta(StatDelta d, BaseGameEntityComponent source, SourceKey key)
        {
            var sr = EnsureStat(d.stat);

            if (d.target == StatTarget.Max)
            {
                sr.effectAddMax += d.amount;
                RecalculateAllMaxAndClampCurrent();
                return;
            }

            float clampMax = sr.maxClamp > 0f ? Mathf.Min(sr.maxClamp, sr.max) : sr.max;
            float newCurrent = Mathf.Clamp(sr.current + d.amount, sr.minClamp, clampMax);
            float delta = newCurrent - sr.current;

            if (delta < 0f)
            {
                _lastHarmSource = source; // запоминаем последний источник вреда для KilledBy

                _damageDrawer?.DrawResourceChange(-delta, isDamage: true,
                    durationOverride: null, type: d.stat);
            }

            SetCurrentInternal(d.stat, newCurrent, key.expendType, key.source);
        }

        private void SetCurrentInternal(ResourceStatType stat, float newCurrent,
            ExpendType type, BaseGameEntityComponent contributionSource)
        {
            var sr = EnsureStat(stat);
            float oldCurrent = sr.current;

            // После Mathf.Clamp точное равенство здесь допустимо
            if (newCurrent == oldCurrent) return;

            sr.current = newCurrent;
            UpdateViewers(stat, sr.current, sr.max, newCurrent - oldCurrent, type, contributionSource);

            // NEW: единая точка проверки условия смерти —
            // сюда стекаются все изменения Current (инстант, периодика, пересчёт Max)
            CheckKillCondition(contributionSource);
        }

        private int NextId() => _nextId++;

        private StatRuntime EnsureStat(ResourceStatType stat)
        {
            if (!stats.TryGetValue(stat, out var sr))
            {
                sr = new StatRuntime();
                stats[stat] = sr;
            }
            return sr;
        }

        #endregion

        #region Periodic & effect ticking

        private bool TickPeriodic(float dt, float now)
        {
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
                if (pr.spec.intervalSeconds <= 0f) pr.spec.intervalSeconds = MinTickInterval;

                int ticks = 0;
                while (pr.accumulator >= pr.spec.intervalSeconds && ticks < MaxTicksPerFrame)
                {
                    pr.accumulator -= pr.spec.intervalSeconds;
                    ticks++;

                    if (!EvaluateConditionGroup(pr.spec.condition)) continue;

                    for (int s = 0; s < pr.stacks; s++)
                    {
                        ApplyDelta(pr.spec.delta, pr.sourceRef, pr.key);
                        anyAppliedTicks = true;
                    }
                }

                // FIX: не даём аккумулятору расти бесконечно при крошечном интервале
                if (ticks >= MaxTicksPerFrame) pr.accumulator = 0f;

                if (_killed) break; // юнит умер от тика — дальше не тикаем
            }

            return anyAppliedTicks;
        }

        private bool ExpireEffects(float now)
        {
            int removed = activeEffects.RemoveAll(ae => ae.expireAt.HasValue && now >= ae.expireAt.Value);
            return removed > 0;
        }

        #endregion

        #region Shields (IShieldReceiver)

        public void AddOrTopUpShield(EffectKey key, ResourceStatType stat, float topUp,
            float coefficient, float absorbLimit, float bufferLifetime)
        {
            var buf = _shields.Find(b => b.Key.Equals(key) && b.Stat == stat);
            if (buf == null)
            {
                buf = new ShieldBuffer(key, stat, coefficient, absorbLimit, bufferLifetime);
                _shields.Add(buf);
            }
            buf.TopUp(topUp, bufferLifetime);
            NotifyShieldViewers(stat);
        }

        public void RemoveShields(EffectKey key)
        {
            HashSet<ResourceStatType> affectedStats = null;
            foreach (var shield in _shields)
            {
                if (shield.Key.Equals(key))
                    (affectedStats ??= new HashSet<ResourceStatType>()).Add(shield.Stat);
            }

            int removed = _shields.RemoveAll(b => b.Key.Equals(key));
            if (removed > 0 && affectedStats != null)
                foreach (var stat in affectedStats)
                    NotifyShieldViewers(stat);
        }

        private void TickShields(float dt)
        {
            HashSet<ResourceStatType> affectedStats = null;

            for (int i = _shields.Count - 1; i >= 0; i--)
            {
                _shields[i].Tick(dt);
                if (_shields[i].IsExpired)
                {
                    (affectedStats ??= new HashSet<ResourceStatType>()).Add(_shields[i].Stat);
                    _shields.RemoveAt(i);
                }
            }

            if (affectedStats == null) return;
            foreach (var stat in affectedStats)
                NotifyShieldViewers(stat);
        }

        private float AbsorbThroughShields(ResourceStatType stat, float damage)
        {
            // FIFO: старейший буфер тратится первым
            bool changed = false;
            for (int i = 0; i < _shields.Count && damage > 0f; i++)
            {
                if (_shields[i].Stat != stat) continue;
                float before = _shields[i].Current;
                damage = _shields[i].Absorb(damage);
                if (!Mathf.Approximately(before, _shields[i].Current)) changed = true;
            }
            if (changed) NotifyShieldViewers(stat);
            return damage;
        }

        #endregion

        #region Max aggregation

        private void RecomputeConditionalFlags()
        {
            _hasConditionalMaxMods =
                liveEquipMaxModifiers.Values.Any(list => list.Any(m => !m.condition.IsEmpty)) ||
                activeEffects.Any(ae => ae.persistentMaxMods.Any(m => !m.condition.IsEmpty));
        }

        private void RecalculateAllMaxAndClampCurrent()
        {
            foreach (var kv in stats)
            {
                kv.Value.equipAddMax = 0f;
                kv.Value.equipMultMax = 0f;
                kv.Value.effectAddMax = 0f;
                kv.Value.effectMultMax = 0f;
            }

            foreach (var kv in liveEquipMaxModifiers)
                foreach (var m in kv.Value)
                {
                    if (!EvaluateConditionGroup(m.condition)) continue;
                    var sr = EnsureStat(m.stat);
                    if (m.op == StatOpKind.Add) sr.equipAddMax += m.value;
                    else sr.equipMultMax += m.value;
                }

            foreach (var ae in activeEffects)
                foreach (var m in ae.persistentMaxMods)
                {
                    if (!EvaluateConditionGroup(m.condition)) continue;
                    var sr = EnsureStat(m.stat);
                    if (m.op == StatOpKind.Add) sr.effectAddMax += m.value;
                    else sr.effectMultMax += m.value;
                }

            foreach (var kv in stats)
            {
                var st = kv.Value;
                float oldMax = st.max;

                float mult = (1f + st.equipMultMax) * (1f + st.effectMultMax);
                st.max = Mathf.Max(0f, (st.baseMax + st.equipAddMax + st.effectAddMax) * mult);

                if (preserveCurrentRatioOnMaxChange && oldMax > 0f)
                    st.current = (st.current / oldMax) * st.max;

                float clampMax = st.maxClamp > 0f ? Mathf.Min(st.maxClamp, st.max) : st.max;
                SetCurrentInternal(kv.Key, Mathf.Clamp(st.current, st.minClamp, clampMax),
                    ExpendType.Equipment, entity);
            }
        }

        #endregion

        #region Condition evaluation

        public bool CheckStatsConditionGroup(ConditionGroup group) => EvaluateConditionGroup(group);

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
                    if (!pass) { result = false; break; }
                }
                else
                {
                    if (pass) { result = true; break; }
                    result = false;
                }
            }

            return group.invert ? !result : result;
        }

        private bool EvaluateCondition(StatCondition c)
        {
            float val;
            if (c.target == StatTarget.Current)
            {
                TryGetCurrent(c.stat, out float cur);
                val = c.usePercentOfMax
                    ? cur / Mathf.Max(0.00001f, GetMax(c.stat)) // normalized 0..1
                    : cur;
            }
            else // Max
            {
                float max = GetMax(c.stat);
                val = c.usePercentOfMax ? 1f : max; // percent-of-max для Max всегда ≈ 1
            }

            const float eps = 0.0001f;
            switch (c.op)
            {
                case ConditionOp.Greater:        return val > c.a;
                case ConditionOp.GreaterOrEqual: return val >= c.a;
                case ConditionOp.Less:           return val < c.a;
                case ConditionOp.LessOrEqual:    return val <= c.a;
                case ConditionOp.Equal:          return Mathf.Abs(val - c.a) <= eps;
                case ConditionOp.NotEqual:       return Mathf.Abs(val - c.a) > eps;
                case ConditionOp.Between:
                    float min = Mathf.Min(c.a, c.b);
                    float maxv = Mathf.Max(c.a, c.b);
                    return val >= min - eps && val <= maxv + eps;
                default: return true;
            }
        }

        #endregion

        #region Public queries

        public bool HasStat(ResourceStatType stat) => stats.ContainsKey(stat);

        public bool TryGetCurrent(ResourceStatType stat, out float value)
        {
            if (stats.TryGetValue(stat, out var sr)) { value = sr.current; return true; }
            value = 0f;
            return false;
        }

        public bool TryGetMax(ResourceStatType stat, out float value)
        {
            if (stats.TryGetValue(stat, out var sr)) { value = sr.max; return true; }
            value = 0f;
            return false;
        }

        public float GetMax(ResourceStatType stat) => stats.TryGetValue(stat, out var sr) ? sr.max : 0f;
        public float GetBaseMax(ResourceStatType stat) => stats.TryGetValue(stat, out var sr) ? sr.baseMax : 0f;

        public bool CanApplyCost(AppliedStatsDeltaEffect cost)
        {
            if (cost?.instantDeltas == null || cost.instantDeltas.Count == 0) return true;

            var neededByStat = new Dictionary<ResourceStatType, float>();
            foreach (var d in cost.instantDeltas)
            {
                if (d.target != StatTarget.Current || d.amount >= 0f) continue;
                neededByStat.TryGetValue(d.stat, out var sum);
                neededByStat[d.stat] = sum + d.amount;
            }

            foreach (var kvp in neededByStat)
            {
                if (!stats.TryGetValue(kvp.Key, out var sr)) return false;
                float required = -kvp.Value;
                float available = Mathf.Max(0f, sr.current - sr.minClamp);
                if (required > available) return false;
            }

            return true;
        }

        #endregion

        #region Viewers

        public void RegisterStatsViewer(IStatUpdatesViewer viewer)
        {
            if (!statUpdatesViewers.Contains(viewer)) statUpdatesViewers.Add(viewer);
            StartViewer(viewer);
        }

        // NEW: симметричная отписка, чтобы не держать уничтоженные UI-объекты
        public void UnregisterStatsViewer(IStatUpdatesViewer viewer)
        {
            statUpdatesViewers.Remove(viewer);
        }

        private void StartViewer(IStatUpdatesViewer viewer)
        {
            foreach (var stat in stats)
                viewer.HandleStatsUpdate(stat.Key, stat.Value.current, stat.Value.max, 0,
                    ExpendType.Equipment, entity);
        }

        private void UpdateViewers(ResourceStatType type, float current, float max, float delta,
            ExpendType expendType, BaseGameEntityComponent source)
        {
            foreach (var v in statUpdatesViewers)
                v.HandleStatsUpdate(type, current, max, delta, expendType, source);
        }

        private void NotifyShieldViewers(ResourceStatType stat)
        {
            // FIX: суммируем только щиты данного стата и дедуплицируем per-stat,
            // раньше total смешивал все статы и один общий _lastNotifiedShield глушил уведомления
            float total = 0f;
            foreach (var shield in _shields)
                if (shield.Stat == stat) total += shield.Current;

            if (_lastNotifiedShieldByStat.TryGetValue(stat, out float last) &&
                Mathf.Approximately(total, last)) return;

            _lastNotifiedShieldByStat[stat] = total;

            foreach (var v in statUpdatesViewers)
                v.SetShieldValue(stat, total);
        }

        #endregion
    }
}