using Arcatech.Items;
using KBCore.Refs;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Units
{
    /// <summary>
    ///  new class to VIEW the data
    /// </summary>
    [RequireComponent(typeof(EntityInventoryComponent))]
    public class EntityInventoryDrawerComponent : ValidatedMonoBehaviour, IUnitInventoryView
    {
        [Self,SerializeField] EntityInventoryComponent inventoryComponent;
        [SerializeField] protected ItemEmpties itemEmpties;
        [SerializeField] protected DrawItemsStrategy defaultItemsDrawStrat;

        bool needsRedraw = true; // meh but it works

        UnitInventoryModel inventoryModel;
        IDrawItemStrategy currentDrawStrategy;

        public event UnityAction ViewChangedInventory = delegate { };


        public void RefreshView(UnitInventoryModel model)
        {

            if (inventoryModel != null && inventoryModel != model)
            {
                // model is changed for some reason
                inventoryModel.DrawStrategyChangedEvent -= OnDrawStrategyChange; 
                inventoryModel = model;
                model.DrawStrategyChangedEvent += OnDrawStrategyChange;
            }
            if (inventoryModel == null)
            {
                // first init
                model.DrawStrategyChangedEvent += OnDrawStrategyChange;
                inventoryModel = model;
            }
            Debug.Log($"Refresh view in inventiory drawer");

        }

        private void OnDrawStrategyChange(IDrawItemStrategy strat)
        {

            currentDrawStrategy = strat;
            Debug.Log($"Running DrawItems()");

            foreach (var e in inventoryModel.ListEquipped)
            {
                ItemPlaceType placeType = strat.GetPlaces[e.Type];
                switch (placeType)
                {
                    case ItemPlaceType.Hidden:
                        e.OnUnequip();
                        break;
                    default:
                        e.SetItemEmpty(itemEmpties.ItemPositions[strat.GetPlaces[e.Type]]);
                        e.OnEquip();
                        break;
                }
            }
        }

        private void Start()
        {
            currentDrawStrategy = defaultItemsDrawStrat;
            inventoryComponent?.SetModelView(this);
        }


    }

}