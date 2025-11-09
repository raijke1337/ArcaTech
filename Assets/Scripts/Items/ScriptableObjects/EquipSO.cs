using Arcatech.Skills;
using Arcatech.Stats;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Items
{
    [Serializable, CreateAssetMenu(fileName = "New Equip Item", menuName = "Items/Equipment")]
    public class EquipSO : ItemSO
    {
        
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