using System;
using Arcatech.Items;
using Arcatech.Triggers;
using Arcatech.Units;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Usables
{
    /// <summary>
    /// apply to user directly
    /// </summary>
    [CreateAssetMenu(fileName = "New Self Hit Producer", menuName = "Usables/Hit Producer/Self target")]
    public class SerializedSelfHitProducer : SerializedHitProducer
    {
        public override IHitProducer Deserialize(BaseGameEntityComponent owner, EquipmentComponent item)
        {
            return new SelfHitProducer(owner,item,this);
        }
    }

    public class SelfHitProducer : HitProducer
    {
        public SelfHitProducer( BaseGameEntityComponent owner, EquipmentComponent item,SerializedSelfHitProducer cfg) : base(owner, item,cfg)
        {
        }

        public override void TriggerEntered(TriggerHitInfo triggerHitInfo)
        {
        }

        public override void TriggerExited(TriggerHitInfo triggerExitInfo)
        {
        }

        public override void OnChangeState(StateMachineNotifyType info)
        {
            switch (info)
            {
                case StateMachineNotifyType.NoNotify:
                    break;
                case StateMachineNotifyType.Starting:
                    break;
                case StateMachineNotifyType.Use:
                    CallHit(new TriggerHitInfo(null,Owner,Owner.EffectSpawn.transform.position,Time.time));
                    break;
                case StateMachineNotifyType.EndUse:
                    break;
                case StateMachineNotifyType.Cancel:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(info), info, null);
            }
            base.OnChangeState(info);
        }

    }
}