using UnityEngine;


namespace Arcatech.Usables.Effects
{
    /// <summary>
    /// Builds a live ActiveEffectInstance from an effect definition + source.
    /// Single place that maps a BaseAppliedEffect subtype to its IEffectResult.
    /// </summary>
    public sealed class EffectFactory
    {
        public ActiveEffectInstance Create(BaseAppliedEffect def, BaseGameEntityComponent source)
        {
            var key = new EffectKey(def.ID, source.GetID);
            var result = BuildResult(def);
            float runnerDuration = def.infiniteDuration && def.periodicity.kind == PeriodicityKind.Repeating
                ? Mathf.Max(0f, def.durationSeconds)
                : (def.infiniteDuration ? float.PositiveInfinity : Mathf.Max(0f, def.durationSeconds));
            var runner = new PeriodicityRunner(def.periodicity, runnerDuration);
            return new ActiveEffectInstance(key, result, source, runner, def.infiniteDuration,def.stackType,def.maxStacks);
        }

        private IEffectResult BuildResult(BaseAppliedEffect def)
        {
            switch (def)
            {
                case AppliedStatsDeltaEffect stats:    return new StatChangeResult(stats);
                case AppliedModifierEffect mod:        return new ModifierResult(mod);
                case AppliedAbsorbShieldEffect shield: return new AbsorbShieldResult(shield);
                case AppliedStunEffect stun:           return new StunResult(stun);
                case AppliedSummonEffect summon:       return new SummonResult(summon);
                default:
                    Debug.LogError($"[EffectFactory] No mapping for {def.GetType().Name} — effect does nothing.");
                    return new NullEffectResult();
            }
        }

    }

    /// <summary> No-op fallback so a missing mapping never crashes the pipeline. </summary>
    public sealed class NullEffectResult : IEffectResult
    {
        public void Apply(EffectContext ctx)
        {
        }

        public void OnExpire(EffectContext ctx)
        {
        }
    }

    public enum DamageDirection { Outgoing, Incoming }

    public interface IDifficultyDamageProvider
    {
        float GetOutgoingMult(BaseGameEntityComponent attacker);
        float GetIncomingMult(BaseGameEntityComponent defender);
    }

    /// <summary> Dummy: no scaling. Replace by wiring GameManager.GetDamageMults(). </summary>
    public sealed class NullDifficultyProvider : IDifficultyDamageProvider
    {
        public float GetOutgoingMult(BaseGameEntityComponent _) => 1f;
        public float GetIncomingMult(BaseGameEntityComponent _) => 1f;
    }
}
