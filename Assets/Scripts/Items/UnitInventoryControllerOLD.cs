using Arcatech.EventBus;
using Arcatech.Skills;
using Arcatech.Stats;
using Arcatech.Triggers;
using Arcatech.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Items
{
    /// <summary>
    /// replaced with EntityInventoryComponent
    /// 
    /// </summary>
    public class UnitInventoryControllerOLD : IManagedController
    {


        //private IUnitInventoryView InventoryView;
        //private UnitInventoryModel Inventory;


        //#region setup


        //public UnitInventoryControllerOLD SetModelView (IUnitInventoryView view)
        //{
        //    if (view != null)
        //    {
        //        InventoryView = view;
        //        InventoryView.RefreshView(Inventory);
        //        view.InventoryChanged += OnInvenoryChangedUI;
        //    }

        //    return this;
        //}

        //public UnitInventoryItemConfigsContainer PackPlayerData()
        //{
        //    List<Item> inv = new List<Item>(Inventory.Inventory.items);
        //    List<Item> eq = new List<Item>(Inventory.Equipments.GetAllValues());

        //    return new UnitInventoryItemConfigsContainer(eq,inv);
        //}


        //#endregion

        //private void OnInvenoryChangedUI()
        //{
        //    // moveto inventory, move to equipped go here)
        //    // events from the view component
        //    Debug.Log($"Something happened in inventory view");
        //}


        //#region used by other components
        //private ItemEmpties _empties;

        //public UnitInventoryControllerOLD(UnitInventoryItemConfigsContainer cfg,ItemEmpties em)
        //{
        //    Inventory = new UnitInventoryModel(cfg, dummyUnit);
        //    _empties= em;
        //    //dummyUnit.StartCoroutine(DelayedUpdate());
        //}

        //public UnitInventoryControllerOLD DrawItems(IDrawItemStrategy strategy)
        //{
        //    if (strategy == null) return this; // case for destructrible items
        //    foreach (var e in Inventory.Equipments.GetAllValues())
        //    {
        //        ItemPlaceType placeType = strategy.GetPlaces[e.Type];
        //        switch (placeType)
        //        {
        //            case ItemPlaceType.Hidden:
        //                e.ItemShown = false;
        //                break;
        //            default:
        //                e.SetItemEmpty(_empties.ItemPositions[strategy.GetPlaces[e.Type]]);
        //                break;
        //        }
        //    }
        //    return this;
        //}

        //public ISkill[] GetSkills
        //{
        //    get
        //    {
        //        List<ISkill> foundSkills = new();
        //        foreach (var e in Inventory.Equipments.GetAllValues())
        //        {
        //            if (e.GetSkill != null)
        //            {
        //                foundSkills.Add(e.GetSkill);
        //            }
        //        }
        //        return foundSkills.ToArray();
        //    }
        //} 

        //public IWeapon[] GetWeapons
        //{
        //    get
        //    {
        //        List<IWeapon> weaps = new();
        //        foreach (var e in Inventory.Equipments.GetAllValues())
        //        {
        //            if (e is IWeapon ww)
        //            {
        //                weaps.Add(ww);                        
        //            }
        //        }
        //        return weaps.ToArray();
        //    }
        //}
        //public bool HasItemType (EquipmentType type, out IEquippable equipment)
        //{
        //    if (Inventory.Equipments.TryGetValue(type, out var  e))
        //    {
        //        equipment = e;
        //        return true;
        //    }
        //    else
        //    {
        //        equipment = null;
        //        return false;
        //    }
        //}
        //public bool HasItem  (ItemSO check)
        //{
        //    return Inventory.HasItem(check);
        //}

        //public StatsMod[] GetCurrentMods
        //{
        //    get
        //    {
        //        var list = new List<StatsMod>();
        //        foreach (var equip in Inventory.Equipments.GetAllValues())
        //        {
        //            if (equip.StatMods != null)
        //            {
        //                list.AddRange(equip.StatMods);
        //            }
        //        }
        //        return list.ToArray();
        //    }
        //}
        //private void InventoryModelUpdated(IEnumerable<IItem> obj)
        //{            
        //    //EventBus<InventoryUpdateEvent>.Raise(new InventoryUpdateEvent(Owner as EquippedUnit,this));
        //}


        //private IEnumerator DelayedUpdate()
        //{
        //    yield return null;
        //    InventoryModelUpdated(Inventory.Equipments.GetAllValues());
        //}
        //#endregion


        //#region managed  - sub to view and model events rhere
        //public override void StartController()
        //{
        //    Inventory.OnInventoryChange += InventoryModelUpdated;
        //    Inventory.OnEquipsChange += InventoryModelUpdated;
        //}

        //public override void ControllerUpdate(float delta)
        //{

        //}

        //public override void StopController()
        //{
        //    Inventory.OnInventoryChange += InventoryModelUpdated;
        //    Inventory.OnEquipsChange -= InventoryModelUpdated;
        //    if (InventoryView != null)
        //    {
        //        InventoryView.InventoryChanged -= OnInvenoryChangedUI;
        //    }
        //}

        //public override void FixedControllerUpdate(float fixedDelta)
        //{
        //}
        //#endregion

        //public bool TryEquipItem (EquipSO e) => Inventory.EquipItem(e, out _);
        public void ControllerUpdate(float delta)
        {
            throw new System.NotImplementedException();
        }

        public void FixedControllerUpdate(float fixedDelta)
        {
            throw new System.NotImplementedException();
        }

        public void StartController()
        {
            throw new System.NotImplementedException();
        }

        public void StopController()
        {
            throw new System.NotImplementedException();
        }
    }
}