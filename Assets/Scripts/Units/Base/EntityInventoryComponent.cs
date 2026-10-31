using System.Collections.Generic;
using System.Linq;
using Arcatech.Items;
using Arcatech.Managers;
using Arcatech.SaveSystem;
using com.cyborgAssets.inspectorButtonPro;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Units
{


    /// <summary>
    /// new class to handle all items associated with an entity. holds the built model.
    /// model is deserialized from saves or loaded from a preset SO.
    /// </summary>
    [RequireComponent(typeof(BaseGameEntityComponent))]
    public class EntityInventoryComponent : ValidatedMonoBehaviour
    {
        [Self, SerializeField] BaseGameEntityComponent baseGameEntity;
        [Space, Header("Items list"), SerializeField]
        protected UnitItemsSO defaultEquips;

        [Tooltip("Включить у игрока: инвентарь берётся из чекпоинта / экипировки «Персонажа» и пишется в прогресс уровня")]
        [SerializeField] private bool useSaveSystem = false;
        private List<IUnitInventoryView> _views;
        [SerializeField] private UnitInventoryModel _model;

        private bool init = false;
        
        private void OnEnable()
        {
            _views??= new();
            IEntityItemsList itemsData = defaultEquips;
            if (useSaveSystem)
            {
                // Чекпоинт -> инвентарь из чекпоинта; новый старт -> экипировка из «Персонажа»;
                // null -> пресет по умолчанию (defaultEquips).
                var saved = LevelProgressController.Instance.GetStartingInventory(gameObject.scene);
                if (saved != null) itemsData = saved;
            }
            _model = new UnitInventoryModel(itemsData, baseGameEntity);
            var views = gameObject.GetComponentsInChildren<IUnitInventoryView>();
            foreach (var view in views)
            {
                SetModelView(view);
            }
            _model.ModelUpdatedEvent += RefreshViews;

            if (useSaveSystem)
            {
                // до подписки на RecordToProgress: костюм из Мастерской - не «найденная на уровне» экипировка
                ApplySavedCostume();
                _model.ModelUpdatedEvent += RecordToProgress;
                RecordSnapshot();
            }
        }

        /// <summary>Костюм, выбранный в Мастерской, надевается поверх стартовой экипировки.
        /// При загрузке с чекпоинта не трогаем: там уже записан костюм, с которым игрок шёл.</summary>
        private void ApplySavedCostume()
        {
            if (LevelProgressController.Instance.ResumedFromCheckpoint) return;

            var id = SaveManager.Instance.MetaProgress.equippedCostumeID;
            if (string.IsNullOrEmpty(id)) return;

            if (!DataManager.Instance.TryGetItemConfig(id, out var config) || config is not EquipSO)
            {
                Debug.LogWarning($"Saved costume '{id}' not found or is not an EquipSO.", this);
                return;
            }

            var item = DataManager.Instance.MakeItem(config, baseGameEntity);
            if (item != null) PickUpItem(item);
        }

        private void RecordToProgress(InventoryChangeNotification notification)
        {
            if (notification.ChangeType == InventoryChangeType.Equip && notification.ChangedItem != null)
                LevelProgressController.TryGetInstance()?.RecordFoundEquipment(notification.ChangedItem.ID);
            RecordSnapshot();
        }

        private void RecordSnapshot()
        {
            LevelProgressController.TryGetInstance()?
                .RecordInventory(SavedEntityInventory.FromModel(_model, baseGameEntity.GetID));
        }


        private void OnDisable()
        {
           // Debug.Log("Inventory disable");
            _model.ModelUpdatedEvent -= RefreshViews;
            _model.ModelUpdatedEvent -= RecordToProgress;
            foreach (var view in _views)
            {
                view.ViewChangedInventory -= HandleViewChange;
            }
            _views.Clear();
        }



        private void RefreshViews(InventoryChangeNotification notification)
        {
            foreach (var view in _views)
            {
                view.RefreshView(notification);
            }

        }

        #region setup

        // views attached to same gameobject are found automatically

        public void SetModelView(IUnitInventoryView view)
        {
            if (view != null)
            {
                _views??= new();
                if (!_views.Contains(view))
                {
                    _views.Add(view);
                    view.RefreshView(new InventoryChangeNotification()
                    {
                        ChangedItem = null,
                        ChangedQuantity = 0,
                        ChangeType = InventoryChangeType.Initialization,
                        InventorySnapshot = _model
                    });
                    view.ViewChangedInventory += HandleViewChange;
                }
                else
                {
                    Debug.LogWarning($"Tried to register {view} twice in {this}");
                }
            }
        }

        /// <summary>Вьюха (меню, UI) просит изменить инвентарь.
        /// Equip - надеть экипировку, PickUp - положить предмет. Остальное пока не поддерживается.</summary>
        private void HandleViewChange(InventoryChangeType type, Item item, int amount)
        {
            if (item == null) return;

            switch (type)
            {
                case InventoryChangeType.Equip when item is Equipment:
                case InventoryChangeType.PickUp:
                    PickUpItem(item, Mathf.Max(1, amount));
                    break;
                default:
                    Debug.LogWarning($"{name}: view request '{type}' is not supported yet.", this);
                    break;
            }
        }

        #endregion

        #region used by other components

        public void PickUpItem(Item item, int amount = 1)
        {
            if (item is Equipment e)
            {
                // equip new or replace equipped
                _model.EquipEquipment(e, out var un);
                if (un != null)
                {
                    // something was dropped
                    
                    // var box = un.PackItem;
                    // box.transform.position = transform.position + transform.forward;
                    // box.gameObject.SetActive(true);
                    // Destroy(un.DisplayItem.gameObject);
                }
            }
            else
            {
                _model.PickUpItem(item, amount);
            }
        }

        public bool TryUseItem(ItemSO what, int amount)
        {
            if (amount == 0)
            {
                bool ok = _model.HasItem(what, amount);
                return ok;
            }

            return _model.UseItem(what, amount);
        }

        #endregion
        
    }

}