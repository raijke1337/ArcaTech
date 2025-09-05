using System.Collections.Generic;

namespace Arcatech.Stats
{
    public abstract class StatUpdateHandlerSerialized : ScriptableObjectID, IStatUpdateHandlingStrategy
    {
        public abstract void HandleUpdate(IDictionary<BaseStatType, StatValueContainer> stats, BaseGameEntityComponent baseEntity, ActiveGameUnitComponent activeEntity);
    }
}
