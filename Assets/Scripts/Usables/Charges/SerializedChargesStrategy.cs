using Arcatech.UI;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Usables
{
    [CreateAssetMenu(fileName = "charges_", menuName = "Usables/Charges/Base (only internal)",order = 0)]
    public class SerializedChargesStrategy : ScriptableObject
    {
        public ActionIconDrawType drawType;
        public float internalCooldown = 0.1f;

        public virtual BasicChargesStrategy Deserialize()
        {
            return new BasicChargesStrategy(this);
        }
    }

    public class BasicChargesStrategy : IReloadStrategy
    {
        
        protected readonly float _intCd;
        protected float _internalCurrent;
        public BasicChargesStrategy(SerializedChargesStrategy charges)
        {
            _intCd = charges.internalCooldown;
            _internalCurrent = charges.internalCooldown;
        }

        public virtual void Tick(float delta)
        {
            if (_internalCurrent > 0) _internalCurrent -=  delta;
        }

        protected virtual bool ReadyCheck()
        {
            return _internalCurrent <= 0;
        }
        
        public bool Ready => ReadyCheck();

        public virtual float FillValue => 0;
        public virtual string DisplayText => "";
        public virtual void OnChangeUsableState(StateMachineNotifyType notifyType)
        {
            switch (notifyType)
            {
                case StateMachineNotifyType.Use:
                    _internalCurrent = _intCd;
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