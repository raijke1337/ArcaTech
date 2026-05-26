using System.Collections.Generic;


namespace Arcatech.Stats
{
    /// <summary>
    /// Holds the runtime state of all stats. Pure data, no logic.
    /// </summary>
    public class StatsRepository
    {
        public readonly struct StatRuntime
        {
            public readonly float BaseMax;
            public readonly float Current;
            public readonly float EffectiveMax;
            public readonly float MinClamp;
            public readonly float MaxClamp;
            public readonly float EquipAddMax;
            public readonly float EquipMultMax;
            public readonly float EffectAddMax;
            public readonly float EffectMultMax;
        }
        
        private readonly Dictionary<ResourceStatType, StatRuntime> stats = new();


        public StatsRepository(BaseStatsConfig cfg)
        {
            
        }
    }
}
