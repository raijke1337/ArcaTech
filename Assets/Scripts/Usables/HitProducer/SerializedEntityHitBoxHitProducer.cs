using Arcatech.Items;
using Arcatech.Triggers;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Usables
{
    [CreateAssetMenu(fileName = "hitProducer_Unithitbox_", menuName = "Usables/Hit Producer/Fixed Unit hitbox")]
    public class SerializedEntityHitBoxHitProducer : SerializedHitProducer
    {
        public override IHitProducer Deserialize(BaseGameEntityComponent owner, EquipmentComponent item)
        {
            return new UnitHitBoxHitProducer(owner, item,this);
        }
        public class UnitHitBoxHitProducer : HitProducer, ITriggerNotificationReceiver
        {
            private ITriggerNotificationProvider provider; 
          
            public UnitHitBoxHitProducer(BaseGameEntityComponent owner, EquipmentComponent item, SerializedHitProducer cfg) : base(owner,item,cfg)
            {
            
                provider = owner.GetComponent<UsablesCasterComponent>().HitArea;
                if (provider == null)
                {
                    Debug.LogError($"{owner.GetName} has no fixed hitbox attached for {item} to cast from");
                    return;
                }
                
                provider.RegisterReceiver(this);
            }

            public override void OnChangeUsableState(StateMachineNotifyType info)
            {

                base.OnChangeUsableState(info);
                if (provider == null) return; 
                switch (info)
                {
                    case StateMachineNotifyType.NoNotify:
                        provider.Active = false;
                        break;
                    case StateMachineNotifyType.Starting:
                        provider.Active = true;
                        break;
                    case StateMachineNotifyType.Use:
                        provider.AreaCast(this);
                        break;
                    case StateMachineNotifyType.EndUse:
                        provider.Active = false;
                        break;
                    case StateMachineNotifyType.Cancel:
                        provider.Active = false;
                        break;
                }
            }
        
            public void TriggerEntered(TriggerHitInfo triggerHitInfo)
            {
                HitCallback(triggerHitInfo);
            }

            public void TriggerExited(TriggerHitInfo triggerExitInfo)
            {
                // HitCallback(triggerExitInfo);
            }

        }
    }
}