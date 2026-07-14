namespace Arcatech.Triggers
{
    public interface ITriggerNotificationProvider : IUsableComponent
    {
        public bool Active { get; set; } 
        public void RegisterReceiver(ITriggerNotificationReceiver receiver);
        public void UnregisterReceiver(ITriggerNotificationReceiver receiver);
        public void AreaCast(ITriggerNotificationReceiver receiver);
        
    }
}