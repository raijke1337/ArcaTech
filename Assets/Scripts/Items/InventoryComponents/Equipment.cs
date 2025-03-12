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

        public Equipment (EquipSO cfg, EquippedUnit ow) : base (cfg,ow)
        {
            DisplayItem = GameObject.Instantiate(cfg.ItemPrefab);
            StatMods = cfg.StatMods;
            if (cfg.Skill!= null)
            {
                GetSkill = cfg.Skill.CreateSkill(ow, DisplayItem, Type);
            }
            DisplayItem.gameObject.SetActive(false);
          //  Debug.Log($"setup equipment{this}");
        }               
        
        public void SetItemEmpty(Transform pos)
        {
            ItemShown = true;            
            DisplayItem.transform.SetParent(pos.transform,false);
        }


        public bool ItemShown
        {
            get { return DisplayItem.gameObject.activeSelf; }
            set
            {
                DisplayItem.gameObject.SetActive(value);
            }
        }

        public BaseItemComponent DisplayItem { get; protected set; }
        public SerializedStatModConfig[] StatMods { get; protected set; }
        public ISkill GetSkill { get; protected set; }
    }
}