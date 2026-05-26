namespace Arcatech.Stats
{
    public interface IStatUpdatesViewer
    {
        public void HandleStatsUpdate (ResourceStatType stat, float statCurrent, float statMax, float statDelta, object changeSource);
    }

}
