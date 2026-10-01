using Arcatech.Items;
using Arcatech.Units;
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
        
        public abstract IHitProducer Deserialize(BaseGameEntityComponent owner, EquipmentComponent item,bool indicateHitBox);
    }

    public abstract class HitProducer : IHitProducer, IAttachable
    {
        protected readonly int MaxHits;
        protected readonly EquipmentComponent Item;
        protected readonly BaseGameEntityComponent Owner;

        protected readonly bool indicateHitBox;
        private int HitsThisUse;  
        
        public HitProducer(BaseGameEntityComponent owner, EquipmentComponent item,SerializedHitProducer cfg, bool indicateHitBox)
        {
            MaxHits = cfg.maxValidHitsPerUse;
            Owner = owner;
            Item = item;
            this.indicateHitBox =  indicateHitBox;
        }


        /// <summary>Предмет экипирован: подключить источники попаданий.</summary>
        public virtual void Attach() { }

        /// <summary>Предмет снят: отключить источники и освободить ресурсы.</summary>
        public virtual void Detach() { }

        public virtual void OnChangeUsableState(StateMachineNotifyType info)
        {
            if (info == StateMachineNotifyType.Starting)
            {
                HitsThisUse = 0;
            }
        }

        public event UnityAction<TriggerHitInfo> EntityHit;
        public event UnityAction<TriggerHitInfo> EnvironmentHit;

        // Наследники не могут вызвать event напрямую - нужны обёртки.
        protected void RaiseEntityHit(TriggerHitInfo info) => EntityHit?.Invoke(info);
        protected void RaiseEnvironmentHit(TriggerHitInfo info) => EnvironmentHit?.Invoke(info);

        protected void HitCallback(TriggerHitInfo info)
        {
            bool hasEntity = info.TryGetEntityTarget(out var entity);
            if (!hasEntity)
            {
                EnvironmentHit?.Invoke(info);
            }
            if (HitsThisUse >= MaxHits) return;
            // FIX: попадание в стену (entity == null) раньше тратило бюджет валидных попаданий
            if (hasEntity && entity != Owner) HitsThisUse++;
            // this is actually a band-aid but should work fine

            EntityHit?.Invoke(info);
        }
    }
}