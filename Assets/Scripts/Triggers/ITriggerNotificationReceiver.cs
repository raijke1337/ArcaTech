namespace Arcatech.Triggers
{
    public interface ITriggerNotificationReceiver
    {
        void TriggerEntered(BaseGameEntityComponent enterComponent, ITriggerNotificationProvider trigger);
        void TriggerExited(BaseGameEntityComponent exitComponent, ITriggerNotificationProvider trigger);
    }
}