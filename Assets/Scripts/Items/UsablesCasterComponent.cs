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
    public class UsablesCasterComponent : ValidatedMonoBehaviour, IUnitCommandHandler, IUnitInventoryView, IDrawItemsStrategyProvider
    {

        public event UnityAction ViewChangedInventory;
        [SerializeField,Self] EntityInventoryComponent entityInventory;
        [SerializeField, Self] private EntityStateMachineComponent _stateUnit;

        public event UnityAction<UnitActionType,bool> ActionAnnounce = delegate { };

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
            _commandValidators = new(GetComponentsInChildren<IUnitCommandValidator>());
            stats =  GetComponent<EntityStatsComponent>();
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

        
        #region commands handler
        private List<IUnitCommandValidator> _commandValidators;
        public void AssignUnitCommandsValidator(IUnitCommandValidator validator)
        {
            if (!_commandValidators.Contains(validator)) _commandValidators.Add(validator);
        }
        


        bool ValidateCommand(UnitActionType type)
        {
            if (!_usables.TryGetValue(type, out var usable)) return false;
            foreach (var validator in _commandValidators)
            {
                if (!validator.CanHandleUnitCommand(type)) return false; // the only validator for now is the grounding component.
                // stats are a separate component because there is a cost attached to item use.
            }

            return stats == null
                ? usable.UsableIsReady()
                : stats.CanApplyCost(usable.GetCost) && usable.UsableIsReady();
        }
        
        public bool DoUnitCommand(UnitActionType type)
        {

            if (!ValidateCommand(type))
            {
                ActionAnnounce.Invoke(type,false);
                return false;
            }
            
            var state = _usables[type].Use();
            _stateUnit.ForceUnitState(state);
            
            if (_usables[type] is IAffectsItemDisplay disp && disp.DrawStrategy != currentDrawItemStrategy)
            {
                currentDrawItemStrategy = disp.DrawStrategy;
                redraw = true;
            }
            stats.ApplyEffect(_usables[type].GetCost,_stateUnit.GetMainEntity);

            ActionAnnounce.Invoke(type,true);
            return true;
        }
        #endregion
        
    }
    
}