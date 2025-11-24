using System.Collections.Generic;
using System.Linq;
using Arcatech.Items;
using Arcatech.Triggers;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Usables
{
    [CreateAssetMenu(fileName = "New Instant Hit Producer", menuName = "Usables/Hit Producer/Instant")]
    public class SerializedInstantHitProducer : SerializedHitProducer
    {
        [Header("No settings in this one")] [HideInInspector]
        private float _a;

        public override IHitProducer Deserialize(BaseGameEntityComponent owner, EquipmentComponent item)
        {
            return new InstantHitProducer(owner, item);
        }
    }
    
        public class InstantHitProducer : IHitProducer
    {
        
        private ITriggerNotificationProvider provider; // From item (e.g., EquipmentComponent's hitbox)

        public InstantHitProducer(BaseGameEntityComponent owner, EquipmentComponent item)
        {
            // Find or create a provider on the item (e.g., attach TriggerTrackerComponent to EquipmentComponent if not present)
            provider = item
                .GetComponent<ITriggerNotificationProvider>(); // Assume pre-attached for melee; create if needed
            if (provider == null)
            {
                // Fallback: Add/reuse a component as provider
                var tracker =
                    item.gameObject
                        .AddComponent<
                            TriggerTrackerComponent>(); // Your TriggerTrackerComponent implementing ITriggerNotificationProvider
                tracker.RecheckCollisions(); // Optional: Force recheck
                provider = tracker;
            }
            provider.RegisterReceiver(this);
        }
        
        public void Initialize()
        {
            provider.Active = true;
        }

        public event UnityAction<TriggerHitInfo> Hit;

        public void Cleanup()
        {
            provider.Active = false;
        }


        public void TriggerEntered(TriggerHitInfo triggerHitInfo)
        {
            Debug.Log("Hit triggered");
            Hit?.Invoke(triggerHitInfo);
        }

        public void TriggerExited(BaseGameEntityComponent exitComponent, ITriggerNotificationProvider trigger)
        {
        }
    }
}