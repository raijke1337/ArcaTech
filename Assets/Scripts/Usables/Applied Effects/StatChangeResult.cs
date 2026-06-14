using System;
using System.Collections.Generic;
using UnityEngine;
using Arcatech.Stats;

namespace Arcatech.Usables.Effects
{
    /// <summary>
    /// Applies the instant stat deltas of an AppliedStatsDeltaEffect through the
    /// target's IStatReceiver (exposed via EffectsReceiverComponent).
    ///
    /// Combat damage (negative Current deltas) is routed through DamagePipeline,
    /// which folds in attacker outgoing-mods, difficulty mults, and defender
    /// incoming-mods exactly once — both sides are known here at apply time.
    ///
    /// NOTE: persistentModifiers and periodicDeltas are EQUIPMENT-ONLY channels
    /// (handled by EntityStatsComponent). Applied effects intentionally ignore them.
    /// </summary>
    public sealed class StatChangeResult : IEffectResult
    {
        private readonly IReadOnlyList<StatDelta> _instantDeltas;

        // cached so we resolve the attacker's outgoing modifiers without re-searching
        private EffectsReceiverComponent _attackerReceiver;
        private BaseGameEntityComponent _cachedAttacker;

        public StatChangeResult(AppliedStatsDeltaEffect cfg)
        {
            _instantDeltas = cfg.instantDeltas != null
                ? (IReadOnlyList<StatDelta>)cfg.instantDeltas
                : Array.Empty<StatDelta>();

#if UNITY_EDITOR
            if ((cfg.persistentModifiers?.Count ?? 0) > 0 || (cfg.periodicDeltas?.Count ?? 0) > 0)
            {
                Debug.LogWarning(
                    $"[StatChangeResult] '{cfg.name}' has persistentModifiers/periodicDeltas " +
                    "(equipment-only channels) which applied effects IGNORE.", cfg);
            }
#endif
        }

        public void Apply(EffectContext ctx)
        {
            if (ctx.TargetReceiver == null) return;
            if (!ctx.TargetReceiver.TryGetStatReceiver(out var receiver)) return;
            if (receiver.Invulnerable) return;

            var attacker = ResolveAttackerReceiver(ctx.Source);
            var defender = ctx.TargetReceiver;
            var key = ctx.Instance != null ? ctx.Instance.Key : default;

            for (int i = 0; i < _instantDeltas.Count; i++)
            {
                var d = _instantDeltas[i];

                // Scale combat damage (negative Current deltas) once, through the single
                // pipeline. Healing / positive deltas pass through untouched (guarded inside).
                d.amount = DamagePipeline.Resolve(d.amount, attacker, defender);

                // TODO (post-MVP): applied-effect conditions are not used per design doc.
                // If ever needed: if (!d.condition.IsEmpty &&
                //     !receiver.CheckStatsConditionGroup(d.condition)) continue;

                receiver.ApplyInstantDelta(d, ctx.Source, key);
            }
        }

        public void OnExpire(EffectContext ctx)
        {
            // Instant stat change has nothing persistent to revert.
        }

        /// <summary>
        /// Finds the attacker's EffectsReceiverComponent (for outgoing damage mods),
        /// caching it since the source is stable for the instance's lifetime.
        /// Returns null for sourceless damage (environmental) — pipeline handles null.
        /// </summary>
        private EffectsReceiverComponent ResolveAttackerReceiver(BaseGameEntityComponent source)
        {
            if (source == null) return null;
            if (ReferenceEquals(source, _cachedAttacker)) return _attackerReceiver;

            _cachedAttacker = source;
            source.TryGetComponent(out _attackerReceiver); // may be null -> outgoing mods skipped
            return _attackerReceiver;
        }
    }
}