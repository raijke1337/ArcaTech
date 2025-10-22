namespace Arcatech.Triggers
{
    public interface ITriggerNotificationReceiver
    {
        void TriggerEntered(BaseGameEntityComponent enterComponent, BaseGameEntityComponent trigger);
        void TriggerExited(BaseGameEntityComponent exitComponent, BaseGameEntityComponent trigger);
    }
}