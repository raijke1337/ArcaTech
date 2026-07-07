
using Arcatech.Items;
using Arcatech.Triggers;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Usables
{
    [CreateAssetMenu(fileName = "hitProducer_WeaponBox_", menuName = "Usables/Hit Producer/Weapon hitbox")]
    public class SerializedWeaponHitBoxHitProducer : SerializedHitProducer
    {
        public override IHitProducer Deserialize(BaseGameEntityComponent owner, EquipmentComponent item)
        {
            return new WeaponHitboxHitProducer(owner, item,this);
        }

    }
    
        public class WeaponHitboxHitProducer : HitProducer, ITriggerNotificationReceiver
        {
            private ITriggerNotificationProvider provider; 
          
            public WeaponHitboxHitProducer(BaseGameEntityComponent owner, EquipmentComponent item, SerializedHitProducer cfg) : base(owner,item,cfg)
            {
            
            provider = item
                .GetComponentInChildren<ITriggerNotificationProvider>(); // Assume pre-attached for melee;
            if (provider == null)
            {
                Debug.LogError($"{item.name} has no hitbox to cast from!");
                return;
            }
            provider.Active = false;
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