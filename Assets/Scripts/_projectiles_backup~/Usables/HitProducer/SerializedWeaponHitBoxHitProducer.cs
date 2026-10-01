
using Arcatech.Items;
using Arcatech.Triggers;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Usables
{
    [CreateAssetMenu(fileName = "hitProducer_WeaponBox_", menuName = "Usables/Hit Producer/Weapon hitbox")]
    public class SerializedWeaponHitBoxHitProducer : SerializedHitProducer
    {
        public override IHitProducer Deserialize(BaseGameEntityComponent owner, EquipmentComponent item,bool indicateHitBox)
        {
            return new WeaponHitboxHitProducer(owner, item,this,indicateHitBox);
        }

    }
    
        public class WeaponHitboxHitProducer : HitProducer, ITriggerNotificationReceiver
        {
            private ITriggerNotificationProvider provider; 
          
            public WeaponHitboxHitProducer(BaseGameEntityComponent owner, EquipmentComponent item, SerializedHitProducer cfg,bool i) : base(owner,item,cfg,i)
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
                if (indicateHitBox) provider.OnChangeUsableState(info);
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
                       // provider.AreaCast(this);
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