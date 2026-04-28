using System;
using System.Collections.Generic;
using System.Linq;
using Arcatech.Items;
using Arcatech.Triggers;
using Arcatech.Units;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Usables
{
    /// <summary>
    /// delivery 
    /// </summary>
    public interface IHitProducer: IUsableComponent
    {
        event UnityAction<TriggerHitInfo> EntityHit;
        event UnityAction<TriggerHitInfo> EnvironmentHit;
    }

    public abstract class SerializedHitProducer : ScriptableObject
    {        
        [Min(0)] public int maxValidHitsPerUse = 1;
        
        public abstract IHitProducer Deserialize(BaseGameEntityComponent owner, EquipmentComponent item);
    }

    public abstract class HitProducer : IHitProducer
    {
        protected readonly int MaxHits;
        protected readonly EquipmentComponent Item;
        protected readonly BaseGameEntityComponent Owner;

        private int HitsThisUse;  
        
        public HitProducer(BaseGameEntityComponent owner, EquipmentComponent item,SerializedHitProducer cfg)
        {
            MaxHits = cfg.maxValidHitsPerUse;
            Owner = owner;
            Item = item;
        }


        public virtual void OnChangeUsableState(StateMachineNotifyType info)
        {
            if (info == StateMachineNotifyType.Starting)
            {
                HitsThisUse = 0;
            }
        }

        public event UnityAction<TriggerHitInfo> EntityHit;
        public event UnityAction<TriggerHitInfo> EnvironmentHit;

        protected void HitCallback(TriggerHitInfo info)
        {
            if (!info.TryGetEntityTarget(out var entity))
            {
                EnvironmentHit?.Invoke(info);
            }
            if (HitsThisUse >= MaxHits) return;
            if (entity != Owner) HitsThisUse++;
            // this is actually a band-aid but should work fine

            EntityHit?.Invoke(info);
        }
    }
}