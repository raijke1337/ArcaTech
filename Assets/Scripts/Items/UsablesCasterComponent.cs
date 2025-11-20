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
        , IDrawItemsStrategyProvider, IStatesAnnounceReceiver
    {

        public event UnityAction ViewChangedInventory;
        [SerializeField,Self] EntityInventoryComponent entityInventory;
        [SerializeField, Self] private EntityStateMachineComponent _stateUnit;

        private EntityStatsComponent stats;
        
        Dictionary<UnitActionType, IUsable> _usables;
        private IUsable _currentUsable;
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
            _currentUsable = null;
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




        public bool CanDoUnitCommand(UnitActionType type, out string info)
        {
            info = $"No usable for action type {type}";
            if (type == UnitActionType.Movement || type == UnitActionType.Jump || type == UnitActionType.Use)
                return true;
            if (!_usables.TryGetValue(type, out var usable)) return false;
            info = "";

            bool ok = false;

            if (stats)
            {
                ok = stats.CanApplyCost(usable.GetCost);
                if (!ok)
                {
                    info = "Can't apply cost";
                    return false;
                }
            }

            ok = usable.UsableIsReady();
            info = ok ? "Ready" : $" {usable.UsableName} Not Ready";
            return ok;
        }
        
        public bool DoUnitCommand(UnitActionType type, bool wasSuccessful)
        {
            if (type == UnitActionType.Movement || type == UnitActionType.Jump || type == UnitActionType.Use) return true;
            
            if (!_usables.TryGetValue(type, out var usable)) return false;
            if (!usable.StartUse()) return false;
            _currentUsable = usable;
        
            if (_usables[type] is IAffectsItemDisplay disp && disp.DrawStrategy != currentDrawItemStrategy)
            {
                currentDrawItemStrategy = disp.DrawStrategy;
                redraw = true;
            }
            
            stats.ApplyEffect(_usables[type].GetCost,_stateUnit.GetMainEntity);
            return true;
        }


        public void OnStateEnter()
        {
        }

        public void OnStateExit()
        {
            _currentUsable?.StopUse();
            _currentUsable = null;
        }
    }

    public interface IStatesAnnounceReceiver
    {
        public void OnStateEnter();
        public void OnStateExit();
    }
}