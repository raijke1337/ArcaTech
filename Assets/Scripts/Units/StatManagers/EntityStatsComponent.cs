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
    /// also uses stat change strategies to affect the rest of components
    /// </summary>
    public class EntityStatsComponent : MonoBehaviour, IUnitInventoryView,IPausableComponent, IKillableComponent,IEffectsTakerComponent
    {
        [SerializeField] protected BaseStatsConfig startingStats;

        [Space, Header("Stats changes handling")]
        [SerializeField] StatsUpdateStrategy[] statsUpdateStrategies;
        IOnStatsChangeStrategy[] _statsStrats;
        
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
            if (!_started) Start();
            
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
            if (!_started) Start(); 
            
            if (!_stats.ContainsKey(mod.GetStatType))
            {
                _stats[mod.GetStatType] = new StatValueContainer();
            }
            _stats[mod.GetStatType].AddStatsMod(mod);
        }




        private void Start()
        {
            if (_started) return;

            _statsStrats = new IOnStatsChangeStrategy[statsUpdateStrategies.Length];
            if (_statsStrats.Any())
            {
                for (int i = 0; i < statsUpdateStrategies.Length; i++)
                {
                    _statsStrats[i] = statsUpdateStrategies[i].BuildStrategy(this);
                }
            }
            else
            {
                Debug.LogWarning($"{this} has no strategies for stats");
            }
            
            _stats = startingStats.BuildBaseStats;
            _startingMods = startingStats.ListMods;
            UpdateViewers();
            _started = true;
            // need to run update once to init containers

        }
        private void Update()
        {
            if (Killed || Paused || !_started) return;
            foreach (var stat in _stats.ToList())
            {
                stat.Value.UpdateInDelta(Time.deltaTime);
                if (!stat.Value.Initialized) // update is done and container not init means that it has no mods at all, so 0/0/0 values
                {
                    _stats.Remove(stat.Key);
                }
            }

            UpdateViewers();
            RunStrats();
        }

        List<IStatUpdatesViewer> _viewers = new();

        public void RegisterStatChangesHandler(IStatUpdatesViewer viewer)
        {
            if (viewer != null && !_viewers.Contains(viewer))
            {
                _viewers.Add(viewer);
                if (_started)
                    viewer.HandleStatsUpdate(_stats);
            }
        }

        void UpdateViewers()
        {
            foreach (var h in _viewers.ToList())
            {
                h.HandleStatsUpdate(_stats);
            }
        }

        void RunStrats()
        {
            foreach (var stat in _statsStrats)
            {
                stat.HandleStats(_stats);
            }
        }

        public bool CanApplyCost(StatsEffect cost)
        {
            if (Killed || Paused || !_started) return false;
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
                UpdateViewers();
                RunStrats();
            }
            else
            {
                Debug.LogError($"tried to apply cost {cost} in {gameObject.name} without checking if its possible");
            }
        }
        private void OnDisable()
        {
            _viewers.Clear();
        }
        
        
        public bool Killed { get; set; } = false;
        public bool Paused { get; set; } = false;

        #region effects taker
        public void ApplyEffect(StatsEffect eff, BaseGameEntityComponent s)
        {
            if (Killed || Paused || eff == null || !_started) return;

            if (_stats.ContainsKey(eff.StatType))
            {
                _stats[eff.StatType].ApplyStatsEffect(eff);
                UpdateViewers();
                RunStrats();
            }
        }
        #endregion
    }
}