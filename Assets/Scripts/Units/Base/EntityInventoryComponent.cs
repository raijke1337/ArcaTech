using Arcatech.Items;
using KBCore.Refs;
using System.Collections.Generic;
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


        private void OnEnable()
        {
            _views = new();
            _model = new UnitInventoryModel(defaultEquips.BuildContainer(),baseGameEntity);

            _model.ItemAddedToInventoryEvent += ModelInventoryChanges;
            _model.ItemRemovedFromInventoryEvent += ModelInventoryChanges;
            _model.ItemEquippedEvent += ModelInventoryChanges;
            _model.ItemUnequippedEvent += ModelInventoryChanges;
        }
        private void ModelInventoryChanges(Item arg0)
        {
            RefreshViews();
        }

        private void OnDisable()
        {
            _model.ItemAddedToInventoryEvent -= ModelInventoryChanges;
            _model.ItemRemovedFromInventoryEvent -= ModelInventoryChanges;
            _model.ItemEquippedEvent -= ModelInventoryChanges;
            _model.ItemUnequippedEvent -= ModelInventoryChanges;
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
            _model.DrawStrategyChangedEvent += RefreshViews;
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
        private void OnInvenoryChangedUI(UnitInventoryViewReference reference)
        {
            Debug.Log($"Something happened in inventory view");
        }

        #endregion

        #region used by other components

        public IUnitActionsHandler GetUnitActionsHandler => _model.Handler;

        //public bool HasItemType(EquipmentType type, out IEquippable equipment)
        //{
        //    if (_model.Equipments.TryGetValue(type, out var e))
        //    {
        //        equipment = e;
        //        return true;
        //    }
        //    else
        //    {
        //        equipment = null;
        //        return false;
        //    }
        //}



        //public bool TryEquipItem(EquipSO e) => _model.EquipItem(e, out _);

        #endregion


    }

}