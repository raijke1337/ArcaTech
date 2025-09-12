using Arcatech.EventBus;
using Arcatech.Items;
using Arcatech.Triggers;
using Arcatech.Units;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
namespace Arcatech.Stats
{
    /// <summary>
    /// new component to handle the current stats and their changes on any game entity
    /// </summary>
    public class EntityStatsComponent : MonoBehaviour, IUnitInventoryView,IPausableComponent
    {
        [SerializeField] protected BaseStatsConfig startingStats;
        [SerializeField] protected float statsUpdateFrequency = 0.1f; // call some events to announce update

        public event UnityAction ViewChangedInventory;
        bool _started = false;
        private Dictionary<BaseStatType, StatValueContainer> _stats;
        private List<StatsMod> _startingMods = new();

        public void RefreshView(UnitInventoryModel m)
        {
            ReloadMods(m.GetCurrentMods);
        }
        void ReloadMods(IEnumerable<StatsMod> mods)
        {
            foreach (var container in _stats.Values)
            {
                container.ResetMods();
            }
            foreach (var initial in _startingMods)
            {
                ApplyStatMod(initial);
            }
            foreach (var mod in mods)
            {
                ApplyStatMod(mod);
            }
        }

        void ApplyStatMod(StatsMod mod)
        {
            if (!_stats.ContainsKey(mod.GetStatType))
            {
                _stats[mod.GetStatType] = new StatValueContainer();
            }
            _stats[mod.GetStatType].AddStatsMod(mod);
        }




        private void Start()
        {
            _stats = startingStats.BuildBaseStats;
            _startingMods = startingStats.ListMods;
            UpdateHandlers();
            _started = true;

        }
        private void Update()
        {
            if (Paused) return;
            foreach (var stat in _stats.ToList())
            {
                stat.Value.UpdateInDelta(Time.deltaTime);
                if (!stat.Value.Initialized) // update is done and container not init means that it has no mods at all, so 0/0/0 values
                {
                    _stats.Remove(stat.Key);
                }
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
        private void OnDisable()
        {
            _handlers.Clear();
        }

        #region pause
        public bool Paused { get; set; } = false;


        #endregion
    }
}