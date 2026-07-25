namespace Arcatech.Stats
{
    public interface IStatUpdatesViewer
    {
        public void HandleStatsUpdate (ResourceStatType stat, float statCurrent, float statMax, float statDelta, EntityStatsComponent.ExpendType changeType, BaseGameEntityComponent source);
        public void SetShieldValue(ResourceStatType shieldStat, float currentValue);
    }

}
