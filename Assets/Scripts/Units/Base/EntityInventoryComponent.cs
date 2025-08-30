using Arcatech.EventBus;
using Arcatech.Items;
using Arcatech.Skills;
using Arcatech.Stats;
using KBCore.Refs;
using System;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

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

            _model.OnInventoryChange += OnInventoryModelUpdated;
            _model.OnEquipsChange += OnInventoryModelUpdated;            
        }

        private void OnDisable()
        {
            _model.OnInventoryChange -= OnInventoryModelUpdated;
            _model.OnEquipsChange -= OnInventoryModelUpdated;
            foreach (var view in _views)
            {
                view.ViewChangedInventory-= OnInvenoryChangedUI;
            }
        }

        private void Update()
        {

            if (baseGameEntity.Paused) return;

            _model?.UpdateModel(Time.deltaTime);
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
        private void OnInventoryModelUpdated(IEnumerable<IItem> obj)
        {
            RefreshViews();
        }

        #endregion

        #region used by other components

        public IUnitActionsHandler GetUnitActionsHandler => _model.Handler;

        public bool HasItemType(EquipmentType type, out IEquippable equipment)
        {
            if (_model.Equipments.TryGetValue(type, out var e))
            {
                equipment = e;
                return true;
            }
            else
            {
                equipment = null;
                return false;
            }
        }
        public bool HasItem(ItemSO check)
        {
            return _model.HasItem(check);
        }

        public StatsMod[] GetCurrentMods
        {
            get
            {
                var list = new List<StatsMod>();
                foreach (var equip in _model.Equipments.GetAllValues())
                {
                    if (equip.StatMods != null)
                    {
                        list.AddRange(equip.StatMods);
                    }
                }
                return list.ToArray();
            }
        }

        public bool TryEquipItem(EquipSO e) => _model.EquipItem(e, out _);


        #endregion


    }

}