using Arcatech.Stats;
using System.Collections.Generic;
using UnityEngine;
namespace Arcatech.Items
{

    public class Equipment : Item, IEquipmentStatsProvider
    {

        public Equipment (EquipSO cfg, BaseGameEntityComponent ow) : base (cfg,ow)
        {
            DisplayItem = GameObject.Instantiate(cfg.itemPrefab,ow.transform);
            DisplayItem.gameObject.SetActive(false);

            mods = new List<StatModifier>(cfg.statModifiers);
            deltas = new List<PeriodicDelta>(cfg.periodicDeltas);
            Slot =  cfg.slot;
        }          
        
        public void SetItemParent(Transform pos)
        {       
            if (!DisplayItem.isActiveAndEnabled) DisplayItem.gameObject.SetActive(true);
            
            DisplayItem.transform.SetParent(pos.transform,false);
        }
        
        public void OnEquip()
        {
            DisplayItem.gameObject.SetActive(true);
        }

        public void OnUnequip()
        {
            DisplayItem.gameObject.SetActive(false);
        }
        public EquipmentComponent DisplayItem { get; protected set; }

        private IEnumerable<StatModifier> mods;
        private IEnumerable<PeriodicDelta> deltas;
        public IEnumerable<StatModifier> GetPersistentModifiers() => mods;
        public IEnumerable<PeriodicDelta> GetPeriodicDeltas() => deltas;
        public BaseGameEntityComponent Source => Owner;


    }

}