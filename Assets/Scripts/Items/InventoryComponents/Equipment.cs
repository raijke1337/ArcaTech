using Arcatech.Stats;
using System.Collections.Generic;
using UnityEngine;
namespace Arcatech.Items
{
    /// <summary>
    /// todo maybe create a separate class for USABLE equipment (we can have equips that give no skill, cosmetic items )
    /// </summary>
    public class Equipment : Item, IEquippable, IHasUsable, IEquipmentStatsProvider
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
            DisplayItem = GameObject.Instantiate(cfg.itemPrefab,ow.transform);
            DisplayItem.gameObject.SetActive(false);

            mods = new List<StatModifier>(cfg.statModifiers);
            deltas = new List<PeriodicDelta>(cfg.periodicDeltas);
            

        }          
        
        public void SetItemEmpty(Transform pos)
        {       
            if (!DisplayItem.isActiveAndEnabled) DisplayItem.gameObject.SetActive(true);
            
            DisplayItem.transform.SetParent(pos.transform,false);
        }

        public EquipmentComponent DisplayItem { get; protected set; }
      //  public List<StatsMod> StatMods { get; protected set; }

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


        private IEnumerable<StatModifier> mods;
        private IEnumerable<PeriodicDelta> deltas;
        public IEnumerable<StatModifier> GetPersistentModifiers() => mods;
        public IEnumerable<PeriodicDelta> GetPeriodicDeltas() => deltas;
        public BaseGameEntityComponent Source => Owner;
    }
}