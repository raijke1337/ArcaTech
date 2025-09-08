using Arcatech.Skills;
using Arcatech.Stats;
using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Items
{
    [Serializable, CreateAssetMenu(fileName = "New Equip Item", menuName = "Items/Equipment")]
    public class EquipSO : ItemSO
    {
        
        public SerializedStatModConfig[] StatMods;
        public SerializedSkill Skill;

        protected override void OnValidate()
        {
            base.OnValidate();
            Assert.IsNotNull(itemPrefab);
          //  Assert.IsNotNull(Skill);
        }

        public override IItem BuildItem(BaseGameEntityComponent owner)
        {
            return new Equipment(this, owner);
        }
    }


}