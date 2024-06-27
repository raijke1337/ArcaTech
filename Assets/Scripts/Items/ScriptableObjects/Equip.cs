using Arcatech.Effects;
using Arcatech.Skills;
using Arcatech.Triggers;
using System;
using UnityEngine;

namespace Arcatech.Items
{
    [Serializable, CreateAssetMenu(fileName = "New Equip Item", menuName = "Items/Equip Item")]
    public class Equip : Item    {

        public BaseEquippableItemComponent ItemPrefab;
        public SerializedStatModConfig[] StatMods;
        public SerializedSkillConfiguration Skill;
        public SerializedEffectsCollection Effects;

    }
}