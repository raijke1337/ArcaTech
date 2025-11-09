using System.Collections.Generic;

namespace Arcatech.Stats
{
    public abstract class SerializedOnEffectApplyStrategy : ScriptableObjectID
    {
        public abstract IStatHandlingStrategy Deserialize(EntityStatsComponent comp);
    }

    public interface IStatHandlingStrategy : IStrategy
    {
        public void StatChanged(ResourceStatType type, float current, float max, float delta,
            object contributionSource);
    }
}
