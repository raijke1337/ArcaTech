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
    public interface IHitProducer : ITriggerNotificationReceiver
    {
        void OnChangeState(StateMachineNotifyType info);
        event UnityAction<TriggerHitInfo> Hit;
    }

    public abstract class SerializedHitProducer : ScriptableObject
    {        
        [Min(0)] public int maxHitsPerUse = 1;
        public bool activateOnSelfHit = false;
        public abstract IHitProducer Deserialize(BaseGameEntityComponent owner, EquipmentComponent item);
    }

    public abstract class HitProducer : IHitProducer
    {
        protected int MaxHits;
        protected readonly EquipmentComponent Item;
        protected readonly BaseGameEntityComponent Owner;
        protected readonly bool SelfHitActivates;
        protected int HitsThisUse;  
        
        
        public HitProducer(BaseGameEntityComponent owner, EquipmentComponent item,SerializedHitProducer cfg)
        {
            MaxHits = cfg.maxHitsPerUse;
            Owner = owner;
            Item = item;
            SelfHitActivates = cfg.activateOnSelfHit;
        }

        public virtual void TriggerEntered(TriggerHitInfo triggerHitInfo)
        {
            if (Owner.ShowingDebugs)
            {
                int counter = triggerHitInfo.IsValidHit ? HitsThisUse + 1 : HitsThisUse;
                
                Debug.Log($"{Item} hit with {triggerHitInfo.Source.GetType()} on {(triggerHitInfo.IsValidHit? triggerHitInfo.Target.GetName : triggerHitInfo.Position)}, " +
                       $"hits this use {counter} out of {MaxHits}. This hit is {(triggerHitInfo.IsValidHit?"valid" : "not valid")}." +
                       $"{((SelfHitActivates && triggerHitInfo.Target == Owner) ? "Owner Trigger Enabled!" : "")}");
            }
            
            if (triggerHitInfo.Target == Owner && !SelfHitActivates) return;
            
            if (triggerHitInfo.IsValidHit) HitsThisUse++;
            if (HitsThisUse > MaxHits)
            {
                return;
            }
            Hit?.Invoke(triggerHitInfo);
        }

        public abstract void TriggerExited(TriggerHitInfo triggerExitInfo);
        public virtual void OnChangeState(StateMachineNotifyType info)
        {
            if (info == StateMachineNotifyType.Starting)
            {
                HitsThisUse = 0;
            }
        }
        protected void CallHit(TriggerHitInfo triggerHitInfo) => Hit?.Invoke(triggerHitInfo);
        public event UnityAction<TriggerHitInfo> Hit;
    }

    
}