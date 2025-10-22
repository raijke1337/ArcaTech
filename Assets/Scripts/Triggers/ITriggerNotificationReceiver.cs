namespace Arcatech.Triggers
{
    public interface ITriggerNotificationReceiver
    {
        void TriggerEntered(BaseGameEntityComponent enterComponent, TriggerTrackerComponent trigger);
        void TriggerExited(BaseGameEntityComponent exitComponent, TriggerTrackerComponent trigger);
    }
}