namespace Arcatech.Triggers
{
    public interface ITriggerNotificationProvider
    {
        public bool Active { get; set; } 
        public void RegisterReceiver(ITriggerNotificationReceiver receiver);
        public void UnregisterReceiver(ITriggerNotificationReceiver receiver);
    }
}