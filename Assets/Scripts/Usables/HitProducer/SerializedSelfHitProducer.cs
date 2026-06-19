using System;
using Arcatech.Items;
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
        public override IHitProducer Deserialize(BaseGameEntityComponent owner, EquipmentComponent item)
        {
            return new SelfHitProducer(owner,item,this);
        }
    }

    public class SelfHitProducer : HitProducer
    {
        private Collider _tgt;
        public SelfHitProducer(BaseGameEntityComponent owner, EquipmentComponent item,SerializedSelfHitProducer cfg) : base(owner, item,cfg)
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
                    HitCallback(new TriggerHitInfo(null, _tgt, Owner.EffectSpawn.position, Vector3.up,
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

    }
}