using Arcatech.EventBus;
using Arcatech.Items;
using Arcatech.Skills;
using Arcatech.Stats;
using KBCore.Refs;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

namespace Arcatech.Units
{


    /// <summary>
    /// new class to handle all items associated with an entity. CONTROLLER
    /// </summary>
    [RequireComponent(typeof(BaseGameEntityComponent))]
    public class EntityInventoryComponent : MonoBehaviour
    {
        [Self, SerializeField] BaseGameEntityComponent baseGameEntity;
        [Space, Header("Items list"), SerializeField] protected UnitItemsSO defaultEquips;


        private List<IUnitInventoryView> InventoryViews;
        private UnitInventoryModel InventoryModel;

        //protected UnitInventoryControllerOLD _inventory;

        private void OnEnable()
        {
            InventoryViews = new();
            InventoryModel = new UnitInventoryModel(defaultEquips.BuildContainer());

            InventoryModel.OnInventoryChange += OnInventoryModelUpdated;
            InventoryModel.OnEquipsChange += OnInventoryModelUpdated;
        }

        private void OnDisable()
        {
            InventoryModel.OnInventoryChange -= OnInventoryModelUpdated;
            InventoryModel.OnEquipsChange -= OnInventoryModelUpdated;
            foreach (var view in InventoryViews)
            {
                view.ViewChangedInventory-= OnInvenoryChangedUI;
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
                if (!InventoryViews.Contains(view))
                {
                    InventoryViews.Add(view);
                    view.RefreshView(InventoryModel);
                    view.ViewChangedInventory += OnInvenoryChangedUI;
                }
            }
        }
        private void OnInvenoryChangedUI()
        {
            // moveto inventory, move to equipped go here)
            // events from the view component
            Debug.Log($"Something happened in inventory view");
        }
        private void OnInventoryModelUpdated(IEnumerable<IItem> obj)
        {
            foreach(var view in InventoryViews)
            {
                view.RefreshView(InventoryModel);
            }
            //EventBus<InventoryUpdateEvent>.Raise(new InventoryUpdateEvent(BaseGameEntityComponent, this));
        }

        //public UnitInventoryItemConfigsContainer PackPlayerData()
        //{
        //    List<Item> inv = new List<Item>(InventoryModel.Inventory.items);
        //    List<Item> eq = new List<Item>(InventoryModel.Equipments.GetAllValues());

        //    return new UnitInventoryItemConfigsContainer(eq, inv);
        //}


        #endregion




        #region used by other components


        public ISkill[] GetSkills
        {
            get
            {
                List<ISkill> foundSkills = new();
                foreach (var e in InventoryModel.Equipments.GetAllValues())
                {
                    if (e.GetSkill != null)
                    {
                        foundSkills.Add(e.GetSkill);
                    }
                }
                return foundSkills.ToArray();
            }
        }

        public IWeapon[] GetWeapons
        {
            get
            {
                List<IWeapon> weaps = new();
                foreach (var e in InventoryModel.Equipments.GetAllValues())
                {
                    if (e is IWeapon ww)
                    {
                        weaps.Add(ww);
                    }
                }
                return weaps.ToArray();
            }
        }
        public bool HasItemType(EquipmentType type, out IEquippable equipment)
        {
            if (InventoryModel.Equipments.TryGetValue(type, out var e))
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
            return InventoryModel.HasItem(check);
        }

        public StatsMod[] GetCurrentMods
        {
            get
            {
                var list = new List<StatsMod>();
                foreach (var equip in InventoryModel.Equipments.GetAllValues())
                {
                    if (equip.StatMods != null)
                    {
                        list.AddRange(equip.StatMods);
                    }
                }
                return list.ToArray();
            }
        }



        #endregion


        public bool TryEquipItem(EquipSO e) => InventoryModel.EquipItem(e, out _);
    }

}