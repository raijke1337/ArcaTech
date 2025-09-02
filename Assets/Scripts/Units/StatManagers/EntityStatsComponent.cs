using Arcatech.EventBus;
using Arcatech.Stats;
using Arcatech.Triggers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Arcatech.Stat
{
    /// <summary>
    /// new component to handle the current stats and their changes on any game entity
    /// </summary>
    public class EntityStatsComponent : MonoBehaviour
    {
        [SerializeField] protected BaseStatsConfig startingStats;
        [SerializeField] protected float statsUpdateFrequency = 0.1f; // call some events to announce update

        #region serialize
        [SerializeField] StatValueContainer[] displayContainers;


        void EditorUpdate()
        {
            displayContainers = new StatValueContainer[_stats.Count-1];
            for (int i = 0; i < displayContainers.Length; i++)
            {
                displayContainers[i] = _stats.ElementAt(i).Value;
            }
        }

        #endregion


        List<IStatUpdatesHandler> _handlers = new();
        public void RegisterStatChangesHandler(IStatUpdatesHandler handler)
        {
            if (handler != null && !_handlers.Contains(handler))
            {
                _handlers.Add(handler);
                if (_started)
                handler.HanldeEntityStatsUpdate(_stats);
            }
        }



        private Dictionary<BaseStatType, StatValueContainer> _stats;
        bool _started = false;
        public bool DidInit => _started;

        private void Start()
        {
            _stats = startingStats.BuildBaseStats;
            _started = true;
            EditorUpdate();
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

        void UpdateHandlers()
        {
            foreach (var h in _handlers)
            {
                h.HanldeEntityStatsUpdate(_stats);
            }
        }
        public void ApplyStatsEffect(StatsEffect eff)
        {
            if (_stats.ContainsKey(eff.StatType))
            {
                _stats[eff.StatType].ApplyStatsEffect(eff);
            }
        }
        public void ApplyStatMod(StatsMod mod)
        {
            _stats[mod.GetStatType].AddStatsMod(mod);
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
        }
        #endregion
    }
}