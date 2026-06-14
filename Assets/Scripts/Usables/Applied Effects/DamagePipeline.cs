namespace Arcatech.Usables.Effects
{
    /// <summary>
    /// Single choke-point for combat damage scaling. Because outgoing damage is
    /// authored as a fixed StatDelta in the effect asset (not built by an attacker
    /// at runtime), both attacker and defender are known at the SAME moment the
    /// delta is applied. So all multipliers resolve here exactly once.
    ///
    /// Order: raw -> attacker outgoing mods -> difficulty -> defender incoming mods
    /// (multiplicative, so order only matters once non-mult steps appear later).
    /// </summary>
    public static class DamagePipeline
    {
        public static IDifficultyDamageProvider Difficulty { get; set; } = new NullDifficultyProvider();

        /// <summary>
        /// Scales a combat amount. Only negative Current deltas (damage) are scaled;
        /// healing/positive deltas pass through untouched.
        /// attacker/defender may be null (environmental, self).
        /// </summary>
        public static float Resolve(float rawAmount,
            EffectsReceiverComponent attacker, EffectsReceiverComponent defender)
        {
            if (rawAmount >= 0f) return rawAmount;
            float amount = rawAmount;
            if (attacker != null && attacker.TryGetModifierAggregator(out var atk))
                amount *= atk.GetMultiplier(ModifierParam.OutgoingDamage);
            amount *= Difficulty.GetOutgoingMult(attacker?.Owner);
            amount *= Difficulty.GetIncomingMult(defender?.Owner);
            if (defender != null && defender.TryGetModifierAggregator(out var def))
                amount *= def.GetMultiplier(ModifierParam.IncomingDamage);
            return amount;
        }
    }
    
}