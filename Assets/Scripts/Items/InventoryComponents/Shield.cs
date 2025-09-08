namespace Arcatech.Items
{
    public class Shield : Equipment
    {
        public Shield(SerializedShieldAbsorbStrategy st, ShieldSO cfg, BaseGameEntityComponent ow) : base(cfg, ow)
        {
            AbsorbStrategy = st.BuildStrategy;
        }
        public ShieldAbsorbStrategy AbsorbStrategy { get; }
    }
}