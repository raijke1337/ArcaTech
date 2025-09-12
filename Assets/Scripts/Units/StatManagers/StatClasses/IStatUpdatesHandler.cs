using System.Collections.Generic;

namespace Arcatech.Stats
{
    public interface IStatUpdatesHandler
    {
        public void HandleStatsUpdate (IDictionary <BaseStatType,StatValueContainer> stats);
    }

}
