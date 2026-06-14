namespace Arcatech.Usables.Effects
{
    /// <summary>
    /// Pure "what happens to the target" strategy.
    /// Knows nothing about timing, stacking or target selection.
    /// One implementation per Result kind from the design doc.
    /// </summary>
    public interface IEffectResult
    {
        /// <summary> Called once per tick (OneShot == single call). </summary>
        void Apply(EffectContext ctx);

        /// <summary>
        /// Called when the owning instance is removed.
        /// Must revert anything persistent (modifier stacks, shield buffers).
        /// No-op for instant results.
        /// </summary>
        void OnExpire(EffectContext ctx);
    }
    
    
    
}