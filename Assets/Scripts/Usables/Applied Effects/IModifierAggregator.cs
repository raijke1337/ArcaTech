namespace Arcatech.Usables.Effects
{
    public enum ModifierParam
    {
        MoveSpeed,
        OutgoingDamage,
        IncomingDamage
    }

    public interface IModifierAggregator
    {
        /// <returns>False if the stack cap was already reached.</returns>
        bool AddStack(ModifierParam param,
            EffectKey key,
            float multiplier,
            ModifierStackCounting counting,
            int maxStacks);

        void RemoveStacks(EffectKey key);
        float GetMultiplier(ModifierParam param);
        int CountStacks(ModifierParam param, EffectKey key);
        int CountStacksByEffectId(ModifierParam param, string effectId);
    }
}