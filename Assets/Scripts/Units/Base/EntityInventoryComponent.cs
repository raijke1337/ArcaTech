using Arcatech.Items;
using Arcatech.Stat;
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

            SetModelView(_model.Handler);
            _model.ModelUpdatedEvent += RefreshViews;
        }


        private void OnDisable()
        {
            _model.ModelUpdatedEvent -= RefreshViews;
            foreach (var view in _views)
            {
                view.ViewChangedInventory-= OnInvenoryChangedUI;
            }
            _views.Clear();
        }

        private void Update()
        {
            if (baseGameEntity.Paused) return;
            _model?.UpdateDeltaModel(Time.deltaTime);
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


        public void SetModelView(IUnitInventoryView view)
        {
            if (view != null)
            {
                if (!_views.Contains(view))
                {
                    _views.Add(view);
                    view.RefreshView(_model);
                    view.ViewChangedInventory += OnInvenoryChangedUI;
                }
            }
        }
        private void OnInvenoryChangedUI()
        {
            Debug.Log($"Something happened in inventory view");
        }

        #endregion

        #region used by other components
        /// <summary>
        /// this is not very good... maybe TODO refactor
        /// </summary>
        public IUnitActionsHandler GetUnitActionsHandler => _model.Handler;

        public void PickUpItem(IItem item, int amount = 1)
        {
            if (item is Equipment e)
            {
                // equip new or replace equipped
                _model.EquipItem(e, out var un);
                if (un != null) // something was dropped
                {
                    if (droppedItemPrefab != null)
                    {
                        var d = Instantiate(droppedItemPrefab,transform.position,Quaternion.identity);
                        d.OverrideResults(new AddItemToInventoryResult(un,1));
                        d.RedrawItem(un.itemPrefab);                        
                    }
                }
            }
            else
            {
                _model.PickUpItem(item as Item);
            }
        }

        #endregion


    }

}