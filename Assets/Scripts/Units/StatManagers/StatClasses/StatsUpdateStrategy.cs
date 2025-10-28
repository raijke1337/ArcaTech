using System.Collections.Generic;

namespace Arcatech.Stats
{
    public abstract class StatsUpdateStrategy : ScriptableObjectID
    {
        public abstract IOnStatsChangeStrategy BuildStrategy(EntityStatsComponent unit);
    }
    public interface IOnStatsChangeStrategy : IStrategy
    {
        public void HandleStats(IDictionary<BaseStatType, StatValueContainer> stats);
    }

    public abstract class StatsChangeHandle : IOnStatsChangeStrategy
    {
        protected EntityStatsComponent unit;
        protected StatsChangeHandle(EntityStatsComponent component) => unit = component;
        public abstract void HandleStats(IDictionary<BaseStatType, StatValueContainer> stats);
    }
    
    
    
}
