using System.Collections.Generic;
using System.Linq;
using Arcatech.Items;
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
    public class EntityInventoryComponent : ValidatedMonoBehaviour, ISaveable
    {
        [ProButton]
        public void DEBUG_WriteItems()
        {
            foreach (var inv in _model.ListInventory)
            {
                Debug.Log(inv.ToString());
            }
        }
        
        
        
        [Self, SerializeField] BaseGameEntityComponent baseGameEntity;

        [Space, Header("Items list"), SerializeField]
        protected UnitItemsSO defaultEquips;

        [SerializeField] private bool useSaveSystem = false;
        private List<IUnitInventoryView> _views;
        [SerializeField] private UnitInventoryModel _model;

        private void OnEnable()
        {
            _views = new();
            IEntityItemsList itemsData = defaultEquips;
            if (useSaveSystem)
            {
                var data = SaveManager.Instance.GetGameData;
                data.TryGetInventoryForEntity(baseGameEntity.GetID, out itemsData);
            }
            _model = new UnitInventoryModel(itemsData, baseGameEntity);
            
            var views = gameObject.GetComponentsInChildren<IUnitInventoryView>();
            foreach (var view in views)
            {
                SetModelView(view);
            }
            _model.ModelUpdatedEvent += RefreshViews;
        }


        private void OnDisable()
        {
            _model.ModelUpdatedEvent -= RefreshViews;
            foreach (var view in _views)
            {
                view.ViewChangedInventory -= HandleViewChange;
            }
            _views.Clear();
        }



        private void RefreshViews()
        {
            foreach (var view in _views)
            {
                view.RefreshView(_model);
            }

        }

        #region setup

        // views attached to same gameobject are found automatically

        public void SetModelView(IUnitInventoryView view)
        {
            if (view != null)
            {
                if (!_views.Contains(view))
                {
                    _views.Add(view);
                    view.RefreshView(_model);
                    view.ViewChangedInventory += HandleViewChange;
                }
                else
                {
                    Debug.LogWarning($"Tried to register {view} twice in {this}");
                }
            }
        }

        private void HandleViewChange()
        {
            Debug.Log($"view changed inventory");
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
                    var box = un.PackItem;
                    box.transform.position = transform.position + transform.forward;
                    box.gameObject.SetActive(true);
                    Destroy(un.DisplayItem.gameObject);
                }
            }
            else
            {
                _model.PickUpItem(item as Item, amount);
            }

            Debug.Log($"Picked up {amount} {item.Description.Title}");
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

        /// <summary>
        /// called by levelprogress mgr on checkpoint reached
        /// </summary>
        public void NotifyForUpdate()
        {
            SaveManager.Instance.UpdateData(this);
        }

        public void PopulateSaveData(GameData data)
        {
            if (!useSaveSystem) return;
            string entityID = baseGameEntity.GetID;
            IReadOnlyDictionary<Item, int> inv = _model.ListInventory;
            IReadOnlyList<Equipment> equips = _model.ListEquipped;

            Dictionary<string, int> savedInventory = new();
            foreach (var pair in inv)
            {
                savedInventory[pair.Key.ID.ToString()] = pair.Value;
            }
            
            List<string> equipsToSave = new();
            foreach (var pair in equips)
            {
                equipsToSave.Add(pair.ID.ToString());
            }
            
            SavedEntityInventory save = new();
            save.EntityID = entityID;
            save.EntityEquipmentIDs =  equipsToSave;
            save.EntityItemIDs = savedInventory.Keys.ToArray();
            save.EntityItemsCount = savedInventory.Values.ToArray();
            
            data.AddOrUpdateInventory(save);
        }
    }

}