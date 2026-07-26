using System.Collections.Generic;
using Arcatech.Stats;
using UnityEngine;
namespace Arcatech.Items
{

    public class Equipment : Item, IEquipmentStatsProvider
    {
        public Equipment (EquipSO cfg, BaseGameEntityComponent ow) : base (cfg,ow)
        {
            mods = new List<StatModifier>(cfg.statModifiers);
            deltas = new List<PeriodicDelta>(cfg.periodicDeltas);
            Slot =  cfg.slot;
            DisplayItem = GameObject.Instantiate(cfg.itemPrefab,ow.transform);
            DisplayItem.gameObject.SetActive(false);
        }          
        
        public void SetItemParent(Transform pos)
        {       
            if (!DisplayItem.isActiveAndEnabled) DisplayItem.gameObject.SetActive(true);
            DisplayItem.transform.SetParent(pos.transform,false);
        }

        public EquipmentComponent DisplayItem { get; protected set; }

        private IEnumerable<StatModifier> mods;
        private IEnumerable<PeriodicDelta> deltas;
        public IEnumerable<StatModifier> GetPersistentModifiers() => mods;
        public IEnumerable<PeriodicDelta> GetPeriodicDeltas() => deltas;
        public BaseGameEntityComponent Source => Owner;
    /// <summary>
    /// called when the item is removed completely
    /// </summary>
        public virtual void OnUnequip()
        { }

    }

}