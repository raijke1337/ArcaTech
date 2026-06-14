namespace Arcatech.Usables.Effects
{

    /// <summary>
    /// Pushes a multiplier stack into the target's aggregator on Apply,
    /// removes it on Expire. Trivial because the "product" formula and
    /// pull-timing live in the aggregator and its consumers.
    /// </summary>
    public sealed class ModifierResult : IEffectResult
    {
        private readonly ModifierParam _param;
        private readonly float _multiplier;
        private bool _applied;

        public ModifierResult(AppliedModifierEffect cfg)
        {
            _param = cfg.param;
            _multiplier = cfg.multiplier;
        }

        public void Apply(EffectContext ctx)
        {
            // A modifier is a persistent stack, not a per-tick action:
            // push exactly once when the instance starts.
            if (_applied) return;
            if (ctx.Target == null || !ctx.TargetReceiver.TryGetModifierAggregator(out var agg)) return;

            agg.AddStack(_param, ctx.Instance.Key, _multiplier);
            _applied = true;
        }

        public void OnExpire(EffectContext ctx)
        {
            if (!_applied) return;
            if (ctx.Target != null && ctx.TargetReceiver.TryGetModifierAggregator(out var agg))
                agg.RemoveStacks(ctx.Instance.Key);
            _applied = false;
        }
    }
}