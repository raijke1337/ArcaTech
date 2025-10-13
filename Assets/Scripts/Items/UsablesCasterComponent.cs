using Arcatech.Skills;
using Arcatech.Stats;
using Arcatech.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Items
{
    /// <summary>
    /// Made this into a separate component for easier use.
    /// Because storing an instance inside inventory model
    /// is a bad idea when you need to add functionality
    /// </summary>
    public class UsablesCasterComponent : ValidatedMonoBehaviour, IUnitCommandHandler, IUnitInventoryView, IDrawItemsStrategyProvider
    {

        public event UnityAction ViewChangedInventory;
        [SerializeField,Self] EntityInventoryComponent entityInventory;

        Dictionary<UnitActionType, IUsable> _usables;
        public List<IUsable> GetUsables
        {
            get
            {
                if (_usables == null) return null;
                else  return _usables.Values.ToList();
            }
        }

        private void Awake()
        {
            _usables = new();
        }

        public void RefreshView(UnitInventoryModel model)
        {
            var newEquips = model.ListEquipped;
            List<IUsable> newList = new();

            foreach (var equipment in newEquips)
            {
                newList.AddRange(equipment.GetUsables);
            }
            foreach (var sk in newList)
            {
                if (!_usables.TryGetValue(sk.UseActionType, out IUsable usable) || usable != sk)
                {
                    // no key or different skill loaded
                    _usables[sk.UseActionType] = sk;
                }
            }
        }

        public void Update()
        {
            foreach (var u in _usables.Values)
            {
                u.DoUpdate(Time.deltaTime);
            }
        }

        public bool TryHandleUnitCommand(UnitActionType type, EntityStatsComponent stats, out UnitState state)
        {
            state = null;
            if (_usables.TryGetValue(type, out var usable))
            {
                if (usable == null)
                {
                    return false;
                }

                bool ok = usable.TryUseItem(stats, out state);


                if (usable is IAffectsItemDisplay disp)
                {
                    if (disp.DrawStrategy != currentDrawItemStrategy)
                    {
                        currentDrawItemStrategy = disp.DrawStrategy;
                        redraw = true;
                    }
                }

                return ok;
            }
            return false;
        }

        #region drawstratprovider

        private bool redraw = false;
        IDrawItemStrategy currentDrawItemStrategy;

        public IDrawItemStrategy GetDrawStrategy
        {
            get
            {
                redraw = false;
                return currentDrawItemStrategy;
            }
        }
        public bool NeedsRedraw => redraw;

        #endregion

    }
    
}