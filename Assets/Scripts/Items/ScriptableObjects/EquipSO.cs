using Arcatech.Skills;
using Arcatech.Stats;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Items
{
    /// <summary>
    /// equipSO has an item that is equipped.
    /// it might have a skill, or not
    /// </summary>
    [Serializable, CreateAssetMenu(fileName = "New Equip Item", menuName = "Items/Equipment")]
    public class EquipSO : ItemSO
    {
        [SerializeField] public EquipmentComponent itemPrefab;
        public SerializedSkill Skill;

        protected override void OnValidate()
        {
            base.OnValidate();
            Assert.IsNotNull(itemPrefab);
        }

        public override IItem BuildItem(BaseGameEntityComponent owner)
        {
            return new Equipment(this, owner);
        }
        
        
        public List <StatModifier> statModifiers;
        public List <PeriodicDelta> periodicDeltas;
    }


}