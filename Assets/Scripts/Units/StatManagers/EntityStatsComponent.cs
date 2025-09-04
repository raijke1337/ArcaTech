using Arcatech.EventBus;
using Arcatech.Items;
using Arcatech.Stats;
using Arcatech.Triggers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
namespace Arcatech.Stat
{
    /// <summary>
    /// new component to handle the current stats and their changes on any game entity
    /// </summary>
    public class EntityStatsComponent : MonoBehaviour, IUnitInventoryView
    {
        [SerializeField] protected BaseStatsConfig startingStats;
        [SerializeField] protected float statsUpdateFrequency = 0.1f; // call some events to announce update


        /// <summary>
        /// used to load equipment mods
        /// </summary>

        UnitInventoryModel model = null;
        bool firstLoad = true;

        public void RefreshView(UnitInventoryModel m)
        {
            if (firstLoad)
            {
                model = m;
                firstLoad = false;
                ///first load
                ApplyStatMods(m.GetCurrentMods);
                model.ItemEquippedEvent += Model_ItemEquippedEvent;
                model.ItemUnequippedEvent += Model_ItemUnequippedEvent;
            }
            else if (m != model)
            {
                /// for some reason the inv model is changed, should bnot happen but...
                Debug.LogError($"Changing unit inventory model for some reason, unhandled!");
            }
        }

        private void Model_ItemUnequippedEvent(Equipment arg0)
        {
            RemoveStatMods(arg0.StatMods);
        }

        private void Model_ItemEquippedEvent(Equipment arg0)
        {
            ApplyStatMods(arg0.StatMods);
        }

        void ApplyStatMod(StatsMod mod)
        {
            if (!_stats.ContainsKey(mod.GetStatType))
            {
                _stats[mod.GetStatType] = new StatValueContainer();
            }
            _stats[mod.GetStatType].AddStatsMod(mod);
        }
        void ApplyStatMods(IEnumerable<StatsMod> mods)
        {
            foreach (StatsMod mod in mods) { ApplyStatMod(mod); } 
        }
        void RemoveStatMod(StatsMod mod)
        {
            _stats[mod.GetStatType].RemoveStatMod(mod);
        }
        void RemoveStatMods(IEnumerable<StatsMod> mods)
        {
            foreach (StatsMod mod in mods) { RemoveStatMod(mod); }
        }




        private Dictionary<BaseStatType, StatValueContainer> _stats;
        bool _started = false;
        public bool DidInit => _started;

        private void Start()
        {
            _stats = startingStats.BuildBaseStats;
            _started = true;
            UpdateHandlers();

        }
        private void Update()
        {
            if (_paused) return;
            foreach (var stat in _stats)
            {
                stat.Value.UpdateInDelta(Time.deltaTime);
            }
            UpdateHandlers();
        }



        List<IStatUpdatesHandler> _handlers = new();
        public void RegisterStatChangesHandler(IStatUpdatesHandler handler)
        {
            if (handler != null && !_handlers.Contains(handler))
            {
                _handlers.Add(handler);
                if (_started)
                    handler.HandleStatsUpdate(_stats);
            }
        }

        void UpdateHandlers()
        {
            foreach (var h in _handlers.ToList())
            {
                h.HandleStatsUpdate(_stats);
            }
        }
        public void ApplyStatsEffect(StatsEffect eff)
        {
            if (_stats.ContainsKey(eff.StatType))
            {
                _stats[eff.StatType].ApplyStatsEffect(eff);
            }
        }

        public bool CanApplyCost(StatsEffect cost)
        {
            bool OK = false;
            if (cost == null)
            {
                OK = true;
            }
            else
            {
                if (_stats.TryGetValue(cost.StatType, out var c))
                {
                    OK = c.GetCurrent >= Mathf.Abs(cost.InitialValue);
                }
            }
            return OK;
        }
        public void ApplyCost(StatsEffect cost)
        {
            var cont = _stats[cost.StatType];
            if (cont.GetCurrent >= Mathf.Abs(cost.InitialValue))
            {
                cont.ApplyStatsEffect(cost);
            }
            else
            {
                Debug.LogError($"tried to apply cost {cost} in {gameObject.name} without checking if its possible");
            }
        }


        #region pause


        EventBinding<PauseToggleEvent> _pauseBind;
        bool _paused;

        public event UnityAction<UnitInventoryViewReference> ViewChangedInventory;

        private void OnEnable()
        {
            _pauseBind = new EventBinding<PauseToggleEvent>(HandlePauseEvent);
            EventBus<PauseToggleEvent>.Register(_pauseBind);
        }

        void HandlePauseEvent(PauseToggleEvent e)
        {
            _paused = e.Value;
        }

        private void OnDisable()
        {
            EventBus<PauseToggleEvent>.Deregister(_pauseBind);
            _handlers.Clear();

            if (model != null)
            {
                model.ItemEquippedEvent -= Model_ItemEquippedEvent;
                model.ItemUnequippedEvent -= Model_ItemUnequippedEvent;
            }
            firstLoad = true;
        }

        #endregion
    }
}