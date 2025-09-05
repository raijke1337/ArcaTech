using System.Collections.Generic;

namespace Arcatech.Stats
{
    public interface IStatUpdatesHandler
    {
        public void HandleStatsUpdate (IDictionary <BaseStatType,StatValueContainer> stats);
    }

    public interface IStatUpdateHandlingStrategy : IStrategy
    {
        public void HandleUpdate(IDictionary<BaseStatType, StatValueContainer> stats, BaseGameEntityComponent baseEntity,ActiveGameUnitComponent activeEntity);
    }


}
