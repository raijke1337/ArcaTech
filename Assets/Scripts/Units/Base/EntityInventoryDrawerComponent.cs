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

        #region held stance

        [Header("Weapon stance")]
        [SerializeField, Tooltip("Какие места отрисовки считаются 'в руках'. Предмет в таком месте определяет стойку (idle и бег).")]
        private ItemPlaceType[] handPlaces;

        public bool HandPlacesConfigured => handPlaces != null && handPlaces.Length > 0;

        /// <summary>Стойка оружия, которое сейчас в руках. null - оружия со стойкой в руках нет.</summary>
        public WeaponStanceSO HeldStance { get; private set; }

        public event UnityAction<WeaponStanceSO> HeldStanceChanged;

        private void UpdateHeldStance(IDrawItemStrategy strat)
        {
            WeaponStanceSO best = null;

            if (strat != null && inventoryModel != null && HandPlacesConfigured)
            {
                foreach (var e in inventoryModel.ListEquipped)
                {
                    if (e.Stance == null) continue;
                    if (System.Array.IndexOf(handPlaces, strat.GetPlaces[e.Slot]) < 0) continue;
                    if (best == null || e.Stance.Priority > best.Priority) best = e.Stance;
                }
            }

            if (best == HeldStance) return;
            HeldStance = best;
            HeldStanceChanged?.Invoke(best);
        }

        #endregion

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
            
            if (inventoryModel == null)
            {
                UpdateHeldStance(strat);   // сбросит стойку
                return;
            }

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

            UpdateHeldStance(strat);
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

        public void OverrideDrawStrategy(IDrawItemStrategy strat)
        {
            currentDrawStrategy = strat;
            DrawItems(currentDrawStrategy);
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