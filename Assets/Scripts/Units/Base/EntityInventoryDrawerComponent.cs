using Arcatech.Items;
using KBCore.Refs;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Units
{
    /// <summary>
    ///  new class to VIEW the data
    /// also draws inventory items in game
    /// </summary>
    [RequireComponent(typeof(EntityInventoryComponent))]
    public class EntityInventoryDrawerComponent : ValidatedMonoBehaviour, IUnitInventoryView
    {
        [Self,SerializeField] EntityInventoryComponent inventoryComponent;
        [SerializeField] protected ItemEmpties itemEmpties;
        [SerializeField] protected DrawItemsStrategy defaultItemsDrawStrat;

        
        #region model view
        
        private UnitInventoryModel inventoryModel;
        public event UnityAction ViewChangedInventory;
        public void RefreshView(UnitInventoryModel model)
        {
            if (inventoryModel != null && inventoryModel != model)
            {
                // model is changed for some reason
                inventoryModel = model;
                DrawItems(defaultItemsDrawStrat);
            }
            if (inventoryModel == null)
            {
                // first init
                inventoryModel = model;
                DrawItems(defaultItemsDrawStrat);
            }
        }
        
        #endregion
        
        #region drawer
        
        private IDrawItemStrategy currentDrawStrategy;
        private IDrawItemsStrategyProvider drawItemsStrategyProvider;

        private void DrawItems(IDrawItemStrategy strat)
        {
            if (strat == currentDrawStrategy) return; // this is probably  checked elsewhere but just in case
            currentDrawStrategy = strat;
            foreach (var e in inventoryModel.ListEquipped)
            {
                ItemPlaceType placeType = strat.GetPlaces[e.Slot];
                if (placeType == ItemPlaceType.Hidden)
                {
                    e.OnUnequip(); 
                }
                else
                {
                    e.OnEquip();
                    e.SetItemParent(itemEmpties.ItemPositions[strat.GetPlaces[e.Slot]]);
                }
            }
        }
        
        #endregion
        protected override void OnValidate()
        {
            base.OnValidate();
            if (GetComponentsInChildren<IDrawItemsStrategyProvider>().Length > 1)
            {
                Debug.LogWarning($"Multiple draw strategy providers on {this.name}");
            }
        }
        private void Start()
        {
            currentDrawStrategy = defaultItemsDrawStrat;
            
            drawItemsStrategyProvider = GetComponentInChildren<IDrawItemsStrategyProvider>();
            if (drawItemsStrategyProvider == null) Debug.Log("No DrawItemsStrategy Provider");
            
            DrawItems(defaultItemsDrawStrat);
        }

        private void Update()
        {
            if (drawItemsStrategyProvider is { NeedsRedraw: true })
            {
                Debug.Log($"Update strategy");
                DrawItems(drawItemsStrategyProvider.GetDrawStrategy);
            }
        }
    }
}