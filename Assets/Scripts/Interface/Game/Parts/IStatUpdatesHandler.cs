using System.Collections;
using System.Collections.Generic;

namespace Arcatech.Stats
{
    public interface IStatUpdatesHandler
    {
        public void HanldeEntityStatsUpdate (IDictionary <BaseStatType,StatValueContainer> stats);
    }
}
