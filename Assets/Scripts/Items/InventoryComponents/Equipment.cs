using Arcatech.Effects;
using Arcatech.Skills;
using Arcatech.Stats;
using Arcatech.Triggers;
using Arcatech.Units;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace Arcatech.Items
{
    /// <summary>
    /// todo maybe create a separate class for USABLE equipment (we can have qeuips that give no skill, cosmetci items )
    /// </summary>
    public class Equipment : Item, IEquippable, IHasUsable
    {
        protected virtual void CollectUsables(EquipSO cfg)
        {
            cachedUsables = new List<IUsable>();
            if (cfg.Skill != null)
            {
                GetUsables.Add(cfg.Skill.CreateSkill(Owner, DisplayItem, Type));
            }
        }
        public Equipment (EquipSO cfg, BaseGameEntityComponent ow) : base (cfg,ow)
        {
            StatMods = new();
            DisplayItem = GameObject.Instantiate(cfg.ItemPrefab);
            if (cfg.StatMods != null)
            {
                foreach (var m in cfg.StatMods)
                {
                    if (m != null)
                        StatMods.Add(m.BuildMod);
                }

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

        protected List<IUsable> cachedUsables;
        public List<IUsable> GetUsables
        {
            get
            {
                if (cachedUsables == null) CollectUsables(Config as EquipSO);
                return cachedUsables;
            }
        }
    }
}