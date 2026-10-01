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
        private WeaponHeatComponent _weaponHeat;
        
        Dictionary<UnitActionType, IUsable> _usables;
        private IUsable _currentUsable;
        public Dictionary<UnitActionType,IUsable> GetUsables => _usables;
        
        private void Awake() 
        {
            _usables??= new();
            _stats = GetComponent<EntityStatsComponent>();
            _weaponHeat = WeaponHeatComponent.Find(this);
        }
        public void RefreshView(InventoryChangeNotification notification)
        {
            if (notification.ChangeType == InventoryChangeType.PickUp ||
                notification.ChangeType == InventoryChangeType.Use) return;
            
            var model = notification.InventorySnapshot;
            _currentUsable = null;

            // новый набор применений: UsablesItem создаёт свои IUsable ОДИН раз в конструкторе,
            // поэтому у предмета, который остался экипированным, это те же самые экземпляры
            var newUsables = new Dictionary<UnitActionType, IUsable>();
            foreach (var item in model.ListEquipped)
            {
                if (item is UsablesItem usablesItem)
                {
                    foreach (var u in usablesItem.GetUsables)
                    {
                        newUsables[u.Key] = u.Value;
                    }
                }
            }

            if (_usables != null)
            {
                foreach (var usable in _usables.Values)
                {
                    if (usable.GetActivationTransition != null)
                        stateUnit.RemoveTransition(usable.GetActivationTransition);
                }
            }
            // Жизненным циклом применений (подписки, пул снарядов) управляет UsablesItem.OnEquip/OnRemove.
            // Вызывать CleanUp здесь нельзя: обновление затрагивает и предметы, оставшиеся экипированными.

            _usables = newUsables;

            foreach (var usable in _usables.Values)
            {
                if (usable.GetActivationTransition != null)
                    stateUnit.AddTransition(usable.GetActivationTransition);
            }

            UpdateWeaponHeat();

            _redraw = true;
        }

        /// <summary>
        /// Перегрев активен, только если среди экипированных применений есть оружие с OverheatStrategy.
        /// Индикатор показывается и прячется по этому признаку (WeaponHeatComponent.Changed).
        /// </summary>
        private void UpdateWeaponHeat()
        {
            if (_weaponHeat == null) return;

            SerializedOverheatStrategy overheat = null;
            foreach (var usable in _usables.Values)
            {
                if (usable is UsableStrategy strategy && strategy.Reload is OverheatStrategy o)
                {
                    overheat = o.Config;
                    break;
                }
            }

            if (_weaponHeat.DebugEnabled)
            {
                var sb = new System.Text.StringBuilder();
                foreach (var pair in _usables)
                {
                    string reload = pair.Value is UsableStrategy s && s.Reload != null
                        ? s.Reload.GetType().Name
                        : pair.Value.GetType().Name;
                    sb.Append($"{pair.Key}={reload}; ");
                }

                _weaponHeat.DebugLog($"UsablesCaster refresh: equipped usables [{sb}] -> overheat weapon: " +
                                     (overheat != null ? overheat.name : "none"));
            }

            _weaponHeat.SetActiveWeapon(overheat);
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