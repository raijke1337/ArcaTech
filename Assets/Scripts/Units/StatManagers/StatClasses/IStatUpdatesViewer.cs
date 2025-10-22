using System.Collections.Generic;

namespace Arcatech.Stats
{
    public interface IStatUpdatesViewer
    {
        public void HandleStatsUpdate (IDictionary <BaseStatType,StatValueContainer> stats);
    }

}
