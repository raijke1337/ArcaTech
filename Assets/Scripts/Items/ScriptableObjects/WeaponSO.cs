using Arcatech.Triggers;
using Arcatech.Units;
using System;
using Arcatech.Stats;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Items
{
    /// <summary>
    ///  a weapon is a type of equipment that also has a weapon use strategy - melee or ranged (also an IUsable)
    /// </summary>
    [Serializable, CreateAssetMenu(fileName = "New Weapon Item", menuName = "Items/Weapon")]
    public class WeaponSO : EquipSO
    {
        [Header("Use settings")]
        public SerializedWeaponUseStrategy WeaponUseStrategy;
        public StatsEffect Cost;
        public DrawItemsStrategy DrawStrategy;
        public SerializedStateTransition StateMachineTransition;

        public override IItem BuildItem(BaseGameEntityComponent owner)
        {
            return new Weapon(this, owner,StateMachineTransition);
        }


    }
}