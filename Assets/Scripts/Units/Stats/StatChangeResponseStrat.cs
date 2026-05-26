using System;
using Arcatech.Units;

namespace Arcatech.Stats
{
    public abstract class StatChangeResponseStrat : ScriptableObjectID
    {
        public abstract IOnStatChange Deserialize(EntityStatsComponent comp);
    }
    
    public interface IOnStatChange : IStrategy
    {
        public void OnStatChanged(ResourceStatType type, float current, float max, float delta,
            object contributionSource);
    }

    [Serializable]
    public struct StatChangePackage
    {
        public ConditionGroup conditionGroup;
        public StatChangeResponseStrat action;
        public SerializedUnitState forceState;

    }
}
