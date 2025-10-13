using Arcatech.Units;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Items
{
    public abstract class SerializedWeaponUseStrategy : ScriptableObject
    {
        [Header("Actions")]
        public SerializedUnitState state;

        [Space,Header("Stats")]
        public int TotalCharges;
        public float ChargeRestoreTime;
        public float InternalCooldown;

        private void OnValidate()
        {
            Assert.IsFalse(TotalCharges == 0);
            Assert.IsNotNull(state);
        }
        public virtual WeaponStrategy ProduceStrategy (BaseGameEntityComponent unit, WeaponSO cfg,BaseEquipmentComponent comp)
        {
            return new WeaponStrategy(state, unit, cfg,TotalCharges,ChargeRestoreTime, InternalCooldown,comp);   
        }
    }


}