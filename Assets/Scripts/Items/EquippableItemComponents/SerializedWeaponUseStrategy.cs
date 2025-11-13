using Arcatech.Units;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Items
{
    public abstract class SerializedWeaponUseStrategy : ScriptableObject
    {
        [Space,Header("Stats")]
        public int TotalCharges;
        public float ChargeRestoreTime;
        public float InternalCooldown = 0.3f;

        public virtual WeaponStrategy ProduceStrategy (BaseGameEntityComponent unit, WeaponSO cfg,EquipmentComponent comp)
        {
            return new WeaponStrategy(unit, cfg,TotalCharges,ChargeRestoreTime, InternalCooldown,comp);   
        }
    }


}