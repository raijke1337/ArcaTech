using JetBrains.Annotations;

namespace Arcatech.Triggers
{
    public interface ITriggerNotificationReceiver
    {
        void TriggerEntered(TriggerHitInfo triggerHitInfo);
        void TriggerExited(TriggerHitInfo triggerExitInfo);
    }
}