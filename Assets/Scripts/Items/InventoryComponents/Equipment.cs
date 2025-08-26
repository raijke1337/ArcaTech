using Arcatech.Effects;
using Arcatech.Skills;
using Arcatech.Stats;
using Arcatech.Triggers;
using Arcatech.Units;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace Arcatech.Items
{

    public class Equipment : Item, IEquippable
    {

        public Equipment (EquipSO cfg, ActiveGameUnitComponent ow) : base (cfg,ow)
        {
            DisplayItem = GameObject.Instantiate(cfg.ItemPrefab);
            foreach (var m in cfg.StatMods)
            {
                StatMods.Add(m.BuildMod);
            }
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
        public List<StatsMod> StatMods { get; protected set; }
        public ISkill GetSkill { get; protected set; }
    }
}