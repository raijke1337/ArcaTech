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
    public interface IHitProducer
    {
        void OnChangeState(StateMachineNotifyType info);
        event UnityAction<TriggerHitInfo> ValidHit;
        event UnityAction<TriggerHitInfo> InvalidHit;
    }

    public abstract class SerializedHitProducer : ScriptableObject
    {        
        [Min(0)] public int maxValidHitsPerUse = 1;
        public bool enemyIsValidHit = true;
        public bool allyIsValidHit = false;
        public bool undefinedIsValidHit = false;
        public bool environmentIsValidHit = false;
        
        public abstract IHitProducer Deserialize(BaseGameEntityComponent owner, EquipmentComponent item);
    }

    public abstract class HitProducer : IHitProducer
    {
        protected readonly int MaxHits;
        protected readonly EquipmentComponent Item;
        protected readonly BaseGameEntityComponent Owner;

        private readonly bool enemyValid;
        private readonly bool allyValid;
        private readonly bool undefinedValid;
        private readonly bool environmentValid;
        
        protected int HitsThisUse;  
        
        public HitProducer(BaseGameEntityComponent owner, EquipmentComponent item,SerializedHitProducer cfg)
        {
            MaxHits = cfg.maxValidHitsPerUse;
            Owner = owner;
            Item = item;

            enemyValid = cfg.enemyIsValidHit;
            allyValid = cfg.allyIsValidHit;
            undefinedValid = cfg.undefinedIsValidHit;
            environmentValid = cfg.environmentIsValidHit;
        }


        public virtual void OnChangeState(StateMachineNotifyType info)
        {
            if (info == StateMachineNotifyType.Starting)
            {
                HitsThisUse = 0;
            }
        }

        public event UnityAction<TriggerHitInfo> ValidHit;
        public event UnityAction<TriggerHitInfo> InvalidHit;

        protected void HitCallback(TriggerHitInfo info)
        { 
            // todo: determine if the hit is valid or not, call the events
            Debug.Log($"Hit {info.Target}");
        }

    }

    
}