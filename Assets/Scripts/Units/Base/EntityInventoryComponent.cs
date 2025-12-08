using Arcatech.Items;
using KBCore.Refs;
using System.Collections.Generic;
using Arcatech.Actions;
using Arcatech.Interactions;
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
        [Space, Header("Items list"), SerializeField] protected UnitItemsSO defaultEquips;


        private List<IUnitInventoryView> _views;
        [SerializeField] private UnitInventoryModel _model;
        [SerializeField] private ActionResultIsProducedInteractionHandler droppedItemPrefab;

        private void OnEnable()
        {
            _views = new();
            _model = new UnitInventoryModel(defaultEquips,baseGameEntity);
            var views = gameObject.GetComponentsInChildren<IUnitInventoryView>();

            foreach (var view in views)
            {
                SetModelView(view);
            }
                
            //SetModelView(_model.casterComponent);
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


        //user uses view to control model
        // model display in view
        // view shows info for user

        #region setup

/// <summary>
/// views attached to same gameobject are found automatically
/// </summary>
/// <param name="view"></param>
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
                _model.EquipItem(e, out var un);
                if (un != null) 
                {
                    // something was dropped
                }
            }
            else
            {
                _model.PickUpItem(item as Item);
            }
        }

        public bool TryUseItem(ItemSO what, int amount)
        {
            if (amount == 0)
            {
                bool ok = _model.HasItem(what);
                Debug.Log($"{(ok? what +" found": "no item:")}");
                return ok;
            }
            else
            {
                return _model.DropItem(what);
            }
        }
        #endregion


    }

}