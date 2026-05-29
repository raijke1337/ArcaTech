using Arcatech.Items;
using KBCore.Refs;
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
            if (inventoryModel != model)
            {
                // model is changed for some reason
                inventoryModel = model;
            }
            DrawItems(defaultItemsDrawStrat);
        }
        
        #endregion
        
        #region drawer
        
        private IDrawItemStrategy currentDrawStrategy;
        private IDrawItemsStrategyProvider drawItemsStrategyProvider;

        private void DrawItems(IDrawItemStrategy strat)
        {
            if (strat == currentDrawStrategy || strat == null) return; // this is probably  checked elsewhere but just in case
//            Debug.Log("DrawItems: " + strat);
            currentDrawStrategy = strat;
            foreach (var e in inventoryModel.ListEquipped)
            {
                ItemPlaceType placeType = strat.GetPlaces[e.Slot];
                if (placeType == ItemPlaceType.Hidden)
                {
                    e.DisplayItem.gameObject.SetActive(false);
                }
                else
                {
                    e.DisplayItem.gameObject.SetActive(true);
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
                Debug.Log($"Update draw items");
                DrawItems(drawItemsStrategyProvider?.GetDrawStrategy);
            }
        }
    }
}