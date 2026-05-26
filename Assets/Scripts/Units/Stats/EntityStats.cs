using System;
using System.Collections.Generic;
using Arcatech.Items;
using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Stats
{
    /// <summary>
    /// new (unused) class placeholder with separate responsibilities
    /// </summary>
    public class EntityStats : ValidatedMonoBehaviour, 
        IAppliedEffectsTakerComponent<AppliedStatsDeltaEffect>, 
        IAppliedEffectsTakerComponent<AppliedStatDeltaAmpEffect>,
        IUnitInventoryView,
        IStateAugmentor
    {
        [SerializeField] private BaseStatsConfig _baseStats;
        [SerializeField] private bool preserveCurrentRatioOnMaxChange = true;

        
        private BaselineStats _stats;
        private AppliedEffectsHolder _effects;
        private void Start()
        {
            _stats = new BaselineStats(_baseStats);
            _effects = new AppliedEffectsHolder();
        }

        private void Update()
        {
            _stats.Tick(Time.deltaTime);
            _effects.Tick(Time.deltaTime);
            _stats.CalculateCurrent(_effects);
           // UpdateViewers();
        }

        public bool ApplyEffect(AppliedStatsDeltaEffect effect, BaseGameEntityComponent source)
        {
            throw new System.NotImplementedException();
        }

        public bool ApplyEffect(AppliedStatDeltaAmpEffect effect, BaseGameEntityComponent source)
        {
            throw new System.NotImplementedException();
        }

        public event UnityAction ViewChangedInventory; // not called because stats doesnt change the inventory
        public void RefreshView(UnitInventoryModel model) => _stats.RefreshView(model);
        
        private List<IStatUpdatesViewer> statUpdatesViewers = new();
        
        public void RegisterStatsViewer(IStatUpdatesViewer viewer)
        {
            if (statUpdatesViewers.Contains(viewer)) return;
            statUpdatesViewers.Add(viewer);
            StartViewer(viewer);
        }

        private void StartViewer(IStatUpdatesViewer viewer)
        {
            // foreach (var stat in stats)
            // {
            //     viewer.HandleStatsUpdate(stat.Key,stat.Value.current,stat.Value.max,0,null);
            // }
        }
        private void UpdateViewers(ResourceStatType type, float current, float max, float delta,
            object contributionSource)
        {
            foreach (var v in statUpdatesViewers)
            {
                v.HandleStatsUpdate(type, current, max, delta, contributionSource);
            }
        }

        public void Attach(IStateAugmentorReceiver machine)
        {
            throw new NotImplementedException();
        }

        public void Detach(IStateAugmentorReceiver machine)
        {
            throw new NotImplementedException();
        }

        public void OnStateEntered(UnitState state, StateMachineContext context)
        {
            throw new NotImplementedException();
        }

        public void OnStateExited(UnitState state, StateMachineContext context)
        {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    public class BaselineStats : IUnitInventoryView
    {
        public event UnityAction ViewChangedInventory;
        private Dictionary<ResourceStatType, StatRuntime> _stats;

        public StatRuntime GetStatSnapshot(ResourceStatType type)
        {
            return _stats[type];
        }
        public void RefreshView(UnitInventoryModel model)
        {
            var newProviders = model.EnumerateProviders();
        }

        public BaselineStats(BaseStatsConfig baseStats)
        {
            // initialize runtimes
        }
        public void CalculateCurrent(AppliedEffectsHolder effects)
        {
            throw new NotImplementedException();
        }

        public void Tick(float deltaTime)
        {
            
        }
    }

    public class AppliedEffectsHolder : IAppliedEffectsTakerComponent<AppliedStatDeltaAmpEffect>,
        IAppliedEffectsTakerComponent<AppliedStatsDeltaEffect>
    {
        private List <AppliedStatsDeltaEffect> _effects;
        private List<AppliedStatDeltaAmpEffect> _amps;
        
        public bool ApplyEffect(AppliedStatDeltaAmpEffect effect, BaseGameEntityComponent source)
        {
            throw new NotImplementedException();
        }

        public bool ApplyEffect(AppliedStatsDeltaEffect effect, BaseGameEntityComponent source)
        {
            throw new NotImplementedException();
        }
        public void Tick(float deltaTime)
        {
            
        }
    }
    
}