using Arcatech.Triggers;

namespace Arcatech.Items
{
    public interface IWeaponHitSource
    {
        public ITriggerNotificationProvider GetTriggerNotificationProvider { get; }
    }
}