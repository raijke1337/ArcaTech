using Arcatech.Triggers;
using UnityEngine;

namespace Arcatech.Levels
{
    public class RoomTransitionController : MonoBehaviour, ITriggerNotificationReceiver
    {
        [SerializeField] private LevelBlockComponent blockA;
        [SerializeField] private LevelBlockComponent blockB;
        public void TriggerEntered(TriggerHitInfo triggerHitInfo)
        {
        }

        public void TriggerExited(TriggerHitInfo triggerExitInfo)
        {
            
        }
    }
}