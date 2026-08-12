using System.Collections.Generic;
using Arcatech.Stats;
using Arcatech.Triggers;
using Arcatech.Units;
using Arcatech.Units.Control;
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
        [SerializeField, Self] private EntityStateMachineComponent stateUnit;
        [SerializeField] private TriggerTrackerComponent meleeHitbox;
        
        public TriggerTrackerComponent HitArea => meleeHitbox;
        private EntityStatsComponent _stats;
        
        Dictionary<UnitActionType, IUsable> _usables;
        private IUsable _currentUsable;
        public Dictionary<UnitActionType,IUsable> GetUsables => _usables;
        
        private void Awake() 
        {
            _usables = new();
            _stats = GetComponent<EntityStatsComponent>();
        }

        public void RefreshView(UnitInventoryModel model)
        {
            _currentUsable = null;
            if (_usables != null)
            {
                foreach (var usable in _usables.Values)
                {
                    if (usable.GetStateTransition != null)
                    stateUnit.RemoveTransition(usable.GetStateTransition);
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
                if (usable.GetStateTransition != null)
                stateUnit.AddTransition(usable.GetStateTransition);
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

        public bool CanDoUnitCommand(UnitCommand command, out string info)
        {
            info = $"No usable for action type {command}";
            if (command.Type == UnitActionType.Movement || command.Type == UnitActionType.Jump || command.Type == UnitActionType.Use)
                return true;
            if (!_usables.TryGetValue(command.Type, out var usable)) return false;
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

        public void PrepareCommand(UnitCommand command)
        {
            if (stateUnit.GetMainEntity.ShowingDebugs && stateUnit.verboseDebugs)  Debug.Log($"[Usables] {Time.time} Prepare {command}");
            if (!_usables.TryGetValue(command.Type, out var usable)) return;
            _currentUsable = usable;
        }

        public void DoUnitCommand(UnitCommand command, bool wasSuccessful)
        {
            if (command.Type is UnitActionType.Movement or UnitActionType.Jump or UnitActionType.Use) return;


            if (stateUnit.GetMainEntity.ShowingDebugs && stateUnit.verboseDebugs)
            {
                Debug.Log($"[Usables] {Time.time} Do {command} success={wasSuccessful}, usable={_currentUsable?.Description.Title ?? "null"}");
            }
            
            if (!wasSuccessful)
            {
                return;
            }
            if (_usables[command.Type].DrawStrategy != null && _usables[command.Type].DrawStrategy != _currentDrawItemStrategy)
            {
                _currentDrawItemStrategy = _usables[command.Type].DrawStrategy;
                _redraw = true;
            }
            _stats.ApplyUsableCost(_usables[command.Type].GetCost,stateUnit.GetMainEntity);
      }

        public void StateMachineNotification(StateMachineNotifyType notifyType)
        {
         //   if (stateUnit.GetMainEntity.ShowingDebugs && stateUnit.verboseDebugs) Debug.Log($"[Usables] {Time.time}Notify {notifyType} in {_currentUsable?.Description.Title}");
            _currentUsable?.Notify(notifyType);
            if (notifyType == StateMachineNotifyType.EndUse) _currentUsable = null;
        }

    }

}