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
        [SerializeField, Tooltip("overlap is done at the effect spawn so 1 should be enough")] private float overlapRadius = 1f;
        public override IHitProducer Deserialize(BaseGameEntityComponent owner, EquipmentComponent item)
        {
            return new SelfHitProducer(overlapRadius, owner,item,this);
        }
    }

    public class SelfHitProducer : HitProducer
    {
        private float _r;
        private Collider[] _c;
        public SelfHitProducer(float rad, BaseGameEntityComponent owner, EquipmentComponent item,SerializedSelfHitProducer cfg) : base(owner, item,cfg)
        {
            _r = rad;
            _c = new Collider[8];
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
                    if (Physics.OverlapSphereNonAlloc(Owner.EffectSpawn.position, _r, _c) > 0)
                    {
                        foreach (var hit in _c)
                        {
                            if (!hit) continue;
                            if (!hit.TryGetComponent(out BaseGameEntityComponent c)) continue;
                            if (c == Owner)
                            {
                                HitCallback(new TriggerHitInfo(null, hit, Owner.EffectSpawn.position, Vector3.up,
                                    Vector3.up, Time.time));
                                break;
                            }
                        }
                    }
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