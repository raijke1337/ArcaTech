﻿using Arcatech.Actions;
using Arcatech.EventBus;
using Arcatech.Units;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Items
{
    [CreateAssetMenu (fileName = "Melee Weapon Use Strategy",menuName = "Items/Use strategy/Melee Weapon") ]
    public class SerializedMeleeWeaponUseStrategy : SerializedWeaponUseStrategy
    {

        [SerializeField] SerializedActionResult[] OnColliderHit;

        private void OnValidate()
        {
            Assert.IsNotNull(OnColliderHit);
            Assert.IsTrue(OnColliderHit.Length > 0);
        }
        public override WeaponStrategy ProduceStrategy(EquippedUnit unit, WeaponSO cfg, BaseWeaponComponent comp)
        {
            return new MeleeWeaponStrategy(OnColliderHit, Action, OnActionStart,OnActionComplete, unit, cfg, TotalCharges,ChargeRestoreTime,comp);
        }
    }


}