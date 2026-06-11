using Arcatech.UI;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Usables
{
    [CreateAssetMenu(fileName = "charges_", menuName = "Usables/Charges/Base (only internal)",order = 0)]
    public class SerializedGenericCooldownStrategy : ScriptableObject
    {
        public float cooldown = 0.1f;

        public virtual BasicChargesStrategy Deserialize()
        {
            return new BasicChargesStrategy(this);
        }
    }

    public class BasicChargesStrategy : IReloadStrategy
    {
        
        protected readonly float cooldown;
        protected float timer;
        public BasicChargesStrategy(SerializedGenericCooldownStrategy charges)
        {
            cooldown = charges.cooldown;
            timer = charges.cooldown;
        }

        public virtual void Tick(float delta)
        {
            if (timer > 0) timer -=  delta;
        }

        protected virtual bool ReadyCheck()
        {
            return timer <= 0;
        }
        
        public bool Ready => ReadyCheck();

        public virtual float FillValue => 0;
        public virtual string DisplayText => "";
        public virtual void OnChangeUsableState(StateMachineNotifyType notifyType)
        {
            switch (notifyType)
            {
                case StateMachineNotifyType.Use:
                    timer = cooldown;
                    break;
            }
        }
    }

    public interface IReloadStrategy : IUsableComponent
    {
        public void Tick(float d);
        public bool Ready { get; }
        public float FillValue { get; }
        public string DisplayText { get; }
    }
    
    
}