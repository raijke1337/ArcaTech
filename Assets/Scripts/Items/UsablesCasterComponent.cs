using Arcatech.Skills;
using Arcatech.Stats;
using Arcatech.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Arcatech.Usables;
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
        , IDrawItemsStrategyProvider, IStateMachineNotificationReceiver
    {

        public event UnityAction ViewChangedInventory;
        [SerializeField,Self] EntityInventoryComponent entityInventory;
        [SerializeField, Self] private EntityStateMachineComponent _stateUnit;

        private EntityStatsComponent _stats;
        
        Dictionary<UnitActionType, IUsable> _usables;
        private IUsable _currentUsable;
        public Dictionary<UnitActionType,IUsable> GetUsables => _usables;

        private void Awake()
        {
            _usables = new();
            _stats =  GetComponent<EntityStatsComponent>();
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

            _usables = new();
            
            var newEquips = model.ListEquipped;
            foreach (var item in newEquips)
            {
                if (item is UsablesItem usablesItem)
                {
                    foreach (var u in usablesItem.GetUsables)
                    {
                        _usables[u.Key] = u.Value;
                    }
                }
            }

            foreach (var usable in _usables.Values)
            {
                _stateUnit.AddTransition(usable.GetStateTransition);
            }

            _redraw = true;
        }

        public void Update()
        {
            foreach (var u in _usables.Values)
            {
                u.DoUpdate(Time.deltaTime);
            }
        }
        

        #region drawstratprovider

        private bool _redraw = false;
        IDrawItemStrategy _currentDrawItemStrategy;

        public IDrawItemStrategy GetDrawStrategy
        {
            get
            {
                _redraw = false;
                return _currentDrawItemStrategy;
            }
        }
        public bool NeedsRedraw => _redraw;

        #endregion




        public bool CanDoUnitCommand(UnitActionType type, out string info)
        {
            info = $"No usable for action type {type}";
            if (type == UnitActionType.Movement || type == UnitActionType.Jump || type == UnitActionType.Use)
                return true;
            if (!_usables.TryGetValue(type, out var usable)) return false;
            info = "";

            bool ok = false;

            if (_stats)
            {
                ok = _stats.CanApplyCost(usable.GetCost);
                if (!ok)
                {
                    info = "Can't apply cost";
                    return false;
                }
            }

            ok = usable.UsableIsReady();
            info = ok ? "Ready" : $" {usable.Description.Title} Not Ready";
            return ok;
        }

        public void PrepareCommand(UnitActionType type)
        {
            if (_stateUnit.GetMainEntity.ShowingDebugs && _stateUnit.verboseDebugs)  Debug.Log($"[Usables] {Time.time} Prepare {type}");
            if (!_usables.TryGetValue(type, out var usable)) return;
            _currentUsable = usable;
        }

        public bool DoUnitCommand(UnitActionType type, bool wasSuccessful)
        {
            if (type is UnitActionType.Movement or UnitActionType.Jump or UnitActionType.Use) return true;


            if (_stateUnit.GetMainEntity.ShowingDebugs && _stateUnit.verboseDebugs)
            {
                Debug.Log($"[Usables] {Time.time} Do {type} success={wasSuccessful}, usable={_currentUsable?.Description.Title ?? "null"}");
            }
            
            if (!wasSuccessful)
            {
                return false;
            }
            if (_usables[type].DrawStrategy != null && _usables[type].DrawStrategy != _currentDrawItemStrategy)
            {
                _currentDrawItemStrategy = _usables[type].DrawStrategy;
                _redraw = true;
            }
            _stats.ApplyEffect(_usables[type].GetCost,_stateUnit.GetMainEntity);
      
            return true;
        }

        public void StateMachineNotification(StateMachineNotifyType notifyType)
        {
            if (_stateUnit.GetMainEntity.ShowingDebugs && _stateUnit.verboseDebugs) Debug.Log($"[Usables] {Time.time}Notify {notifyType} in {_currentUsable?.Description.Title}");
            _currentUsable?.Notify(notifyType);
            if (notifyType == StateMachineNotifyType.EndUse) _currentUsable = null;
        }

    }

}