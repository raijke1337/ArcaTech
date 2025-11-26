using Arcatech.Skills;
using Arcatech.Stats;
using Arcatech.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        
        public bool DoUnitCommand(UnitActionType type, bool wasSuccessful)
        {
            if (!wasSuccessful) return false;
            if (type == UnitActionType.Movement || type == UnitActionType.Jump || type == UnitActionType.Use) return true;
            
            if (!_usables.TryGetValue(type, out var usable)) return false;
            
            _currentUsable = usable;
        
            if (_usables[type] is IAffectsItemDisplay disp && disp.DrawStrategy != _currentDrawItemStrategy)
            {
                Debug.Log("Set strategy");
                _currentDrawItemStrategy = disp.DrawStrategy;
                _redraw = true;
            }
            _currentUsable.StartUse();
            _stats.ApplyEffect(_usables[type].GetCost,_stateUnit.GetMainEntity);
            return true;
        }

        public void StateMachineNotification(StateMachineNotifyType notifyType)
        {
            _currentUsable.Notify(notifyType);
        }
    }

}