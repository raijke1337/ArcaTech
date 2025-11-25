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
        [Header("Activate once, then turn off")]
        
        public bool activateOnSelfHit = false;
        public LayerMask layerMask;
        public override IHitProducer Deserialize(BaseGameEntityComponent owner, EquipmentComponent item)
        {
            return new InstantHitProducer(this, owner, item,activateOnSelfHit);
        }
    }
    
        public class InstantHitProducer : IHitProducer
        {
            private bool _onSelfHitActivates;
            private BaseGameEntityComponent _owner;
            private int _layerMask;
        
        private ITriggerNotificationProvider provider; // From item (e.g., EquipmentComponent's hitbox)

        public InstantHitProducer(SerializedInstantHitProducer cfg, BaseGameEntityComponent owner, EquipmentComponent item,bool onSelf)
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
            _onSelfHitActivates = onSelf;
            _owner = owner;
            _layerMask = cfg.layerMask;
            provider.LayerMaskIndex = _layerMask;
            
            provider.Active = false;
            provider.RegisterReceiver(this);
        }
        
        public void Initialize()
        {
            Debug.Log("Initializing Instant Hit Producer");
            provider.Active = true;
        }

        public event UnityAction<TriggerHitInfo> Hit;

        public void Cleanup()
        {
            Debug.Log("Cleaning up Instant Hit Producer");
            provider.Active = false;
        }


        public void TriggerEntered(TriggerHitInfo triggerHitInfo)
        {
            if (triggerHitInfo.Target == _owner && !_onSelfHitActivates) return;
            if (triggerHitInfo.IsValidHit)
            {
                Cleanup();
            }
            Hit?.Invoke(triggerHitInfo);
        }

        public void TriggerExited(BaseGameEntityComponent exitComponent, ITriggerNotificationProvider trigger)
        {
        }
    }
}