using System.Collections.Generic;

namespace Arcatech.Stats
{
    public interface IEquipmentStatsProvider
    {
        IEnumerable<StatModifier> GetPersistentModifiers(); // e.g., Hammer: +100 to Stam Max
        IEnumerable<PeriodicDelta> GetPeriodicDeltas();     // e.g., Shield: +1 Energy Current per second
        BaseGameEntityComponent Source { get; }
    }
}