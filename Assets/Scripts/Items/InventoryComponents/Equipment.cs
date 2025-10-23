using Arcatech.Stats;
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
            if (ow.ShowingDebugs) Debug.Log($"Build equipment class {this}");
            StatMods = new();
            if (cfg.StatMods != null)
            {
                foreach (var m in cfg.StatMods)
                {
                    if (m) StatMods.Add(m.BuildMod);
                }
            }   
            DisplayItem = GameObject.Instantiate(cfg.itemPrefab,ow.transform);
            DisplayItem.gameObject.SetActive(false);
        }          
        
        public void SetItemEmpty(Transform pos)
        {       
            if (!DisplayItem.isActiveAndEnabled) DisplayItem.gameObject.SetActive(true);
            
            DisplayItem.transform.SetParent(pos.transform,false);
        }

        public EquipmentComponent DisplayItem { get; protected set; }
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



        public void OnEquip()
        {
            DisplayItem.gameObject.SetActive(true);
        }

        public void OnUnequip()
        {
            DisplayItem.gameObject.SetActive(false);
        }
    }
}