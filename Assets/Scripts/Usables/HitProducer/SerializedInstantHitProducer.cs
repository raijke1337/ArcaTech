using System;
using System.Collections.Generic;
using System.Linq;
using Arcatech.Items;
using Arcatech.Triggers;
using Arcatech.Units;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Usables
{
    [CreateAssetMenu(fileName = "hitProducer_", menuName = "Usables/Hit Producer/Instant")]
    public class SerializedInstantHitProducer : SerializedHitProducer
    {
        public override IHitProducer Deserialize(BaseGameEntityComponent owner, EquipmentComponent item)
        {
            return new InstantHitProducer(owner, item,this);
        }
    }
    
        public class InstantHitProducer : HitProducer
        {
            private ITriggerNotificationProvider provider; // From item (e.g., EquipmentComponent's hitbox)
          
            public InstantHitProducer(BaseGameEntityComponent owner, EquipmentComponent item, SerializedHitProducer cfg) : base(owner,item,cfg)
            {
            
            provider = item
                .GetComponentInChildren<ITriggerNotificationProvider>(); // Assume pre-attached for melee;
            if (provider == null)
            {
                // Fallback: Add/reuse a component as provider
                var tracker =
                    item.gameObject
                        .AddComponent<
                            TriggerTrackerComponent>(); 

                provider = tracker;
            }
            provider.Active = false;
            provider.RegisterReceiver(this);
        }

        public override void OnChangeState(StateMachineNotifyType info)
        {

            base.OnChangeState(info);
            switch (info)
            {
                case StateMachineNotifyType.NoNotify:
                    provider.Active = false;
                    break;
                case StateMachineNotifyType.Starting:
                    provider.Active = true;
                    break;
                case StateMachineNotifyType.Use:
                    provider.Active = true;
                    break;
                case StateMachineNotifyType.EndUse:
                    provider.Active = false;
                    break;
                case StateMachineNotifyType.Cancel:
                    provider.Active = false;
                    break;
            }
        }


        public override void TriggerExited(TriggerHitInfo triggerExitInfo)
        {
        }
    }
}