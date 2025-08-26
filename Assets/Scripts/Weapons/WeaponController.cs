using Arcatech.Effects;
using Arcatech.EventBus;
using Arcatech.Items;
using Arcatech.Stats;
using Arcatech.Triggers;
using KBCore.Refs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Arcatech.Units
{
    [Serializable]
    public class WeaponController : ManagedControllerBase, ICombatActions
    {

        protected Dictionary<UnitActionType, IWeapon> _weapons;
        protected UnitStatsControllerOLD _stats;

        private UnitInventoryController _inv;
        private EventBinding<InventoryUpdateEvent> bindInv;

        IWeapon _currentWeapon;

        public WeaponController(UnitStatsControllerOLD stats, UnitInventoryController inv, EquippedUnit dummyUnit) : base(dummyUnit)
        {
            _stats = stats;
            _inv = inv;
            bindInv = new EventBinding<InventoryUpdateEvent>(OnInventoryUpdate);
            UpdateWeapons(_inv);    
        }

        private void OnInventoryUpdate(InventoryUpdateEvent obj)
        {
            UpdateWeapons(_inv);
        }

        void UpdateWeapons(UnitInventoryController i)
        {
            _weapons = new();
            foreach (var weapon in i.GetWeapons)
            {
                _weapons[weapon.UseActionType] = weapon;
            }
        }
        public bool CheckUnitArmed (out IWeapon w)
        {
            if (_currentWeapon != null)
            {
                w = _currentWeapon;
                return true;
            }
            else if (_weapons.Count > 0)
            {
                w = _weapons.First().Value; return true;
            }
            else 
            {
                w = null;
                return false; 
            }
        }

        #region interface
        public bool TryUseAction(UnitActionType action, out BaseUnitAction onUse)
        {
            onUse = null;
            if (ActionAvailable(action))
            {
                bool ok = _weapons[action].TryUseItem(_stats, out onUse);
                if (ok)
                {
                    _currentWeapon = _weapons[action];
                    _inv.DrawItems(_weapons[action].DrawStrategy);
                    if (DebugMessage && Owner.UnitDebug) { Debug.Log($"{Owner} used weapon {_weapons[action]}"); }
                    return true;
                }
            }
            if (DebugMessage && Owner.UnitDebug) { Debug.Log($"{Owner} failed to use weapon {_weapons[action]}"); }
            return false;
        }

        public bool ActionAvailable(UnitActionType action)
        {
            return _weapons.ContainsKey(action);
        }
        public bool CanUseAction(UnitActionType action)
        {
            return _weapons[action].CanUseItem(_stats);
        }
        #endregion


        #region managed
        public override void StartController()
        {
            EventBus<InventoryUpdateEvent>.Register(bindInv);
        }
        public override void ControllerUpdate(float delta)
        {
            foreach (var w in _weapons.Values)
            { w.DoUpdate(delta); }
        }

        public override void FixedControllerUpdate(float fixedDelta)
        {

        }

        public override void StopController()
        {
            EventBus<InventoryUpdateEvent>.Deregister(bindInv);
        }



        #endregion
    }

}