using Arcatech.Stats;

namespace Arcatech.Usables.Effects
{
    /// <summary> Marker for components that can receive effect-driven changes. </summary>
    public interface IEffectReceiver
    {
    }

    /// <summary>
    /// Receives instant stat deltas. Implemented by EntityStatsComponent.
    /// The lifecycle (timing, ticks, stacks) lives in EntityEffectController,
    /// so this just applies a single delta when asked.
    /// </summary>
    public interface IStatReceiver : IEffectReceiver,IInvulnerability
    {
        bool ApplyInstantDelta(StatDelta delta, BaseGameEntityComponent source, EffectKey key);

    }
    public interface IShieldReceiver : IEffectReceiver
    {
        void AddOrTopUpShield(EffectKey key, ResourceStatType stat, float topUp,
            float coefficient, float absorbLimit, float bufferLifetime);
        void RemoveShields(EffectKey key);
    }
}