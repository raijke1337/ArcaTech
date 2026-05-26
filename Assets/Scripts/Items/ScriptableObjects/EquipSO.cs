using System;
using System.Collections.Generic;
using Arcatech.Stats;
using UnityEngine;

namespace Arcatech.Items
{
    /// <summary>
    /// equipSO has an item that is equipped.
    /// it might have a skill, or not
    /// </summary>
    [Serializable, CreateAssetMenu(fileName = "equipment_", menuName = "Items/Equipment")]
    public class EquipSO : ItemSO
    {
        [SerializeField] public EquipmentComponent itemPrefab;
        public ItemSlot slot;


        public List <StatModifier> statModifiers;
        public List <PeriodicDelta> periodicDeltas;
        
        public override Item BuildItem(BaseGameEntityComponent owner)
        {
            return new Equipment(this, owner);
        }
        
        
    }


}