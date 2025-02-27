using Arcatech.Items;
using Arcatech.Triggers;
using System.Linq;
using UnityEngine;

namespace Arcatech.Units
{
    
    public class EquippedUnit : BaseEntity
    {
        [Space,Header("Equipped Unit"), SerializeField] protected UnitItemsSO defaultEquips;
        [SerializeField] protected ItemEmpties itemEmpties;
        [SerializeField] protected DrawItemsStrategy defaultItemsDrawStrat;

        protected UnitInventoryController _inventory;

        public override void StartControllerUnit()
        {

            base.StartControllerUnit();


            _inventory = new UnitInventoryController(SelectSerializedItemsConfig(), itemEmpties, this);
            _inventory.DrawItems(defaultItemsDrawStrat)
                .StartController();

            _stats.AddMods(_inventory.GetCurrentMods);


        }

        public override void DisableUnit()
        {
            base.DisableUnit();
            UnitPaused = true;
            _inventory.StopController();
            _stats.StopController();

        }


        public override void RunUpdate(float delta)
        {
            base.RunUpdate(delta);
            _inventory.ControllerUpdate(delta);
        }

        public override void RunFixedUpdate(float delta)
        {

            base.RunFixedUpdate(delta);
            _inventory.FixedControllerUpdate(delta);
        }



        #region inventory

        protected virtual UnitInventoryItemConfigsContainer SelectSerializedItemsConfig()
        {
            return new UnitInventoryItemConfigsContainer(defaultEquips);
        }

        public bool HasItem(ItemSO toCheck) => _inventory.HasItem(toCheck);
        #endregion

/// <summary>
/// shield check
/// </summary>
/// <param name="eff"></param>
        public override void ApplyEffect(StatsEffect eff, IEquippable shieldModifier, out float current)
        {
            if( _inventory.HasItemType(EquipmentType.Shield, out var s))
            {
                base.ApplyEffect(eff,s,out current);
            }
            else
            {
                base.ApplyEffect(eff,null,out current);
            }
        }


    }

}