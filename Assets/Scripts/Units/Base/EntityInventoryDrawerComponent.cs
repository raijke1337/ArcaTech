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
            currentDrawStrategy = strat;
            foreach (var e in inventoryModel.ListEquipped)
            {
                ItemPlaceType placeType = strat.GetPlaces[e.Type];
                switch (placeType)
                {
                    case ItemPlaceType.Hidden:
                        e.OnUnequip();
                        break;
                    default:
                        try
                        {
                            e.SetItemEmpty(itemEmpties.ItemPositions[strat.GetPlaces[e.Type]]);
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine($"No empties set in {this}");
                        }
                        e.OnEquip();
                        break;
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

            // if (itemEmpties.ItemPositions.TryGetValue(ItemPlaceType.RangedEmpty,out var r)) 
            //     r.forward =  transform.forward; // makes sure weapon is pointing forward alongside the player model and mouse
        }

        private void Update()
        {
            if (drawItemsStrategyProvider != null && drawItemsStrategyProvider.NeedsRedraw)
            {
                DrawItems(drawItemsStrategyProvider.GetDrawStrategy);
            }
        }
    }
}