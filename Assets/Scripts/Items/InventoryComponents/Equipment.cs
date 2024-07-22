using Arcatech.Effects;
using Arcatech.Skills;
using Arcatech.Triggers;
using Arcatech.Units;
using System;
using UnityEngine;
namespace Arcatech.Items
{
    public class Equipment : Item, IEquippable
    {

        public Equipment (EquipSO cfg, DummyUnit ow) : base (cfg,ow)
        {
            DisplayItem = GameObject.Instantiate(cfg.ItemPrefab);
            StatMods = cfg.StatMods;
            GetSkill = cfg.Skill.CreateSkill(ow, DisplayItem);
            DisplayItem.gameObject.SetActive(false);
          //  Debug.Log($"setup equipment{this}");
        }               
        
        public void SetItemEmpty(Transform pos)
        {
            ItemShown = true;

            DisplayItem.transform.SetPositionAndRotation(pos.position, pos.rotation);
            DisplayItem.transform.SetParent(pos.transform);
        }


        public bool ItemShown
        {
            get { return DisplayItem.gameObject.activeSelf; }
            set
            {
                DisplayItem.gameObject.SetActive(value);
            }
        }

        public BaseEquippableItemComponent DisplayItem { get; protected set; }
        public SerializedStatModConfig[] StatMods { get; protected set; }
        public ISkill GetSkill { get; protected set; }
    }
}