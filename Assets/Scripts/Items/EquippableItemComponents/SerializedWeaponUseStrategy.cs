﻿using Arcatech.Actions;
using Arcatech.Triggers;
using Arcatech.Units;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Arcatech.Items
{
    public abstract class SerializedWeaponUseStrategy : ScriptableObject
    {
        [Header("Actions")]
        public SerializedUnitAction Action;

        [Space,Header("Stats")]
        public int TotalCharges;
        public float ChargeRestoreTime;
        public float InternalCooldown;

        private void OnValidate()
        {
            Assert.IsFalse(TotalCharges == 0);
            Assert.IsNotNull(Action);
        }
        public virtual WeaponStrategy ProduceStrategy (EquippedUnit unit, WeaponSO cfg,BaseWeaponComponent comp)
        {
            return new WeaponStrategy(Action, unit, cfg,TotalCharges,ChargeRestoreTime, InternalCooldown,comp);   
        }
    }


}