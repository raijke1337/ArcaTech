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
    [RequireComponent(typeof(EntityStateMachineComponent),typeof(UnitInputsComponent))]
    public class UsablesCasterComponent : ValidatedMonoBehaviour, IUnitCommandPerformer, IUnitInventoryView,IUnitCommandValidator
        , IDrawItemsStrategyProvider
    {

        public event UnityAction ViewChangedInventory;
        [SerializeField,Self] EntityInventoryComponent entityInventory;
        [SerializeField, Self] private EntityStateMachineComponent _stateUnit;

        private EntityStatsComponent stats;
        
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
            stats =  GetComponent<EntityStatsComponent>();
        }

        public void RefreshView(UnitInventoryModel model)
        {
            if (_usables != null)
            {
                foreach (var usable in _usables.Values)
                {
                    _stateUnit.RemoveTransition(usable.GetStateTransition);
                }
            }
            
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
            foreach (var usable in _usables.Values)
            {
                _stateUnit.AddTransition(usable.GetStateTransition);
            }
        }

        public void Update()
        {
            foreach (var u in _usables.Values)
            {
                u.DoUpdate(Time.deltaTime);
            }
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

        


        bool ValidateCommand(UnitActionType type)
        {
            if (type == UnitActionType.Movement || type == UnitActionType.Jump) return true;
            if (!_usables.TryGetValue(type, out var usable)) return false;

            return stats == null
                ? usable.UsableIsReady()
                : stats.CanApplyCost(usable.GetCost) && usable.UsableIsReady();
        }
        public bool CanDoUnitCommand(UnitActionType type) => ValidateCommand(type);
        public bool DoUnitCommand(UnitActionType type, bool wasSuccessful)
        {
            if (type == UnitActionType.Movement || type == UnitActionType.Jump) return true;
            
            if (!_usables.TryGetValue(type, out var usable)) return false;
            {
                if (!usable.Use()) return false;
            }
            if (_usables[type] is IAffectsItemDisplay disp && disp.DrawStrategy != currentDrawItemStrategy)
            {
                currentDrawItemStrategy = disp.DrawStrategy;
                redraw = true;
            }
            
            stats.ApplyEffect(_usables[type].GetCost,_stateUnit.GetMainEntity);
            return true;
        }


    }
    
}