using Arcatech.Items;
using KBCore.Refs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Units
{
    [RequireComponent(typeof(EntityInventoryComponent))]
    public class EntityInventoryDrawerComponent : ValidatedMonoBehaviour, IUnitInventoryView
    {
        [Self, SerializeField] EntityInventoryComponent inventoryComponent;
        [SerializeField] protected ItemEmpties itemEmpties;
        [SerializeField] protected DrawItemsStrategy defaultItemsDrawStrat;

        // Храним список активных объектов, чтобы скрывать их при смене инвентаря
        private HashSet<GameObject> _activeDisplayItems = new HashSet<GameObject>();

        #region model view
        
        private UnitInventoryModel inventoryModel;
        public event UnityAction ViewChangedInventory;

        public void RefreshView(InventoryChangeNotification notification)
        {
            if (notification.InventorySnapshot == null) return;

            if (inventoryModel != notification.InventorySnapshot)
            {
                inventoryModel = notification.InventorySnapshot;
            }
            
            // Всегда перерисовываем при изменении инвентаря, сохраняя текущую стратегию (например, если игрок целится)
            DrawItems(currentDrawStrategy ?? defaultItemsDrawStrat);
        }
        
        #endregion
        
        #region drawer
        
        private IDrawItemStrategy currentDrawStrategy;
        private IDrawItemsStrategyProvider drawItemsStrategyProvider;

        private void DrawItems(IDrawItemStrategy strat)
        {
            if (strat == null) return; 

            // 1. Скрываем все ранее активные предметы (решает проблему "старое оружие остается видимым")
            foreach (var go in _activeDisplayItems)
            {
                if (go != null) go.SetActive(false);
            }
            _activeDisplayItems.Clear();

            currentDrawStrategy = strat;
            
            if (inventoryModel == null) return;

            // 2. Отрисовываем текущее экипированное оружие
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
                    _activeDisplayItems.Add(e.DisplayItem.gameObject);
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
                DrawItems(drawItemsStrategyProvider?.GetDrawStrategy);
            }
        }
    }
}