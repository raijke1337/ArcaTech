namespace Arcatech.Usables.Effects
{

    /// <summary>
    /// Pushes a multiplier stack into the target's aggregator on Apply,
    /// removes it on Expire. Trivial because the "product" formula and
    /// pull-timing live in the aggregator and its consumers.
    /// </summary>
    public sealed class ModifierResult : BaseResult
    {
        private readonly ModifierParam         _param;
        private readonly float                 _multiplier;
        private readonly ModifierStackCounting _counting;
        private readonly int                   _maxStacks;
        private bool _applied;

        public ModifierResult(AppliedModifierEffect cfg): base(cfg)
        {
            _param      = cfg.param;
            _multiplier = cfg.multiplier;
            _counting   = cfg.counting;
            _maxStacks  = cfg.maxStacks;   // MaxStacks уже есть в BaseAppliedEffect
        }

        public override void Apply(EffectContext ctx)
        {
            if (_applied) return;
            if (ctx.Target == null ||
                !ctx.TargetReceiver.TryGetModifierAggregator(out var agg)) return;

            // AddStack вернёт false если cap достигнут — не помечаем как применённый
            _applied = agg.AddStack(_param, ctx.Instance.Key,
                _multiplier, _counting, _maxStacks);
        }

        public override void OnExpire(EffectContext ctx)
        {
            if (!_applied) return;
            if (ctx.Target != null &&
                ctx.TargetReceiver.TryGetModifierAggregator(out var agg))
                agg.RemoveStacks(ctx.Instance.Key);
            _applied = false;
        }
    }
}