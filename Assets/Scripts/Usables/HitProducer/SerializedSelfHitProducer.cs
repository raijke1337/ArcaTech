using System;
using Arcatech.Items;
using Arcatech.Triggers;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Usables
{
    /// <summary>
    /// apply to user directly
    /// </summary>
    [CreateAssetMenu(fileName = "hitProducer_self_", menuName = "Usables/Hit Producer/Self target")]
    public class SerializedSelfHitProducer : SerializedHitProducer
    {
        public override IHitProducer Deserialize(BaseGameEntityComponent owner, EquipmentComponent item, bool indicateHitBox)
        {
            return new SelfHitProducer(owner,item,this,indicateHitBox);
        }
    }

    public class SelfHitProducer : HitProducer, ITriggerNotificationProvider
    {
        private Collider _tgt;
        public SelfHitProducer(BaseGameEntityComponent owner, EquipmentComponent item,SerializedSelfHitProducer cfg,bool indicateHitBox) : base(owner, item,cfg,indicateHitBox)
        {
            _tgt = owner.GetComponent<Collider>();
        }
        
        public override void OnChangeUsableState(StateMachineNotifyType info)
        {
            switch (info)
            {
                case StateMachineNotifyType.NoNotify:
                    break;
                case StateMachineNotifyType.Starting:
                    break;
                case StateMachineNotifyType.Use:
                    HitCallback(new TriggerHitInfo(this, _tgt, Owner.EffectSpawn.position, Vector3.up,
                        Vector3.up, Time.time));
                    break;
                case StateMachineNotifyType.EndUse:
                    break;
                case StateMachineNotifyType.Cancel:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(info), info, null);
            }
            base.OnChangeUsableState(info);
        }

        public bool Active { get => true; set => throw new NotImplementedException(); }
        public void RegisterReceiver(ITriggerNotificationReceiver receiver)
        { throw new NotImplementedException(); }

        public void UnregisterReceiver(ITriggerNotificationReceiver receiver)
        {throw new NotImplementedException(); }

        public void AreaCast(ITriggerNotificationReceiver receiver)
        {throw new NotImplementedException(); }
    }
}