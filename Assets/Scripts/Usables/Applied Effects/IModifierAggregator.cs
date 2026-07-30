namespace Arcatech.Usables.Effects
{
    public enum ModifierParam
    {
        MoveSpeed,
        OutgoingDamage,
        IncomingDamage
    }

    /// <summary>
    /// Holds multiplicative modifier stacks on an entity. Effects push/remove stacks;
    /// consumers (movement, damage calc) PULL the final multiplier when needed.
    /// </summary>
    public interface IModifierAggregator : IEffectReceiver
    {
        void AddStack(ModifierParam param, EffectKey key, float multiplier);
        void RemoveStacks(EffectKey key);

        /// <summary> Product of all live stacks for this param (1.0 if none). </summary>
        float GetMultiplier(ModifierParam param);

        /// <summary> Live stack count under one EffectKey (per-source counting). </summary>
        int CountStacks(ModifierParam param, EffectKey key);

        /// <summary> Live stack count for an effectId across all sources (on-target counting). </summary>
        int CountStacksByEffectId(ModifierParam param, string effectId);
    }

    
}
