using Arcatech.Stats;

namespace Arcatech.Usables.Effects
{
    public sealed class AbsorbShieldResult : BaseResult
    {
        private readonly ResourceStatType _stat;
        private readonly float _valuePerTick;
        private readonly float _coefficient;
        private readonly float _absorbLimit;
        private readonly float _bufferLifetime;

        public AbsorbShieldResult(AppliedAbsorbShieldEffect cfg): base(cfg)
        {
            _stat = cfg.absorbedStat;
            _valuePerTick = cfg.absorbValuePerTick;
            _coefficient = cfg.absorbCoefficient;
            _absorbLimit = cfg.absorbLimit;
            _bufferLifetime = cfg.bufferLifetimeSeconds;
        }

        public override void Apply(EffectContext ctx)
        {
            // each tick tops up the buffer (capped by absorbLimit inside the buffer)
            if (ctx.Target == null || !ctx.TargetReceiver.TryGetShieldReceiver(out var rec)) return;
            rec.AddOrTopUpShield(ctx.Instance.Key, _stat, _valuePerTick,
                _coefficient, _absorbLimit, _bufferLifetime);
        }

        public override void OnExpire(EffectContext ctx)
        {
            // Buffer dies with the effect (design decision): clear this effect's buffers.
            if (ctx.Target != null && ctx.TargetReceiver.TryGetShieldReceiver(out var rec))
                rec.RemoveShields(ctx.Instance.Key);
        }
    }
}