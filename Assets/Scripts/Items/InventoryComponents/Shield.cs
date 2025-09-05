namespace Arcatech.Items
{
    public class Shield : Equipment
    {
        public Shield(ShieldSO cfg, BaseGameEntityComponent ow) : base(cfg, ow)
        {
            AbsorbStrategy = cfg.absorbStrategy.ProduceStrat();
        }
        public ShieldAbsorbStrategy AbsorbStrategy { get; }
    }
}