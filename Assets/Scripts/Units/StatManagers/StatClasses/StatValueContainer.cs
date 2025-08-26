
using Arcatech.Triggers;
using Arcatech.Units.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Arcatech.Stats
{
    [Serializable]
    public class StatValueContainer
    {
        #region public
        /// <summary>
        /// <see langword="false"/>means the container has no mods and 0 value
        /// </summary>
        public bool Initialized { get => _setup; } 
        public float GetCurrent { get => _currentValue; }
        public float GetMax { get => _maxValue; }
        public float GetMin { get => _minValue; }
        public float GetPercent { get => _currentValue / _maxValue; }
        public float CachedValue { get => _cachedValue; } // to store changes between updates
        #endregion

        private float _currentValue;
        private float _cachedValue;
        private float _maxValue;
        private float _minValue = 0f;
        private float _initValue = 0f;
        private bool _setup = false;

        public override string ToString()
        {
            return ($"{Mathf.RoundToInt(GetCurrent)} / {Mathf.RoundToInt(GetMax)}");
        }
        public StatValueContainer()
        {
            _currentEffects = new();
            _currentMods = new();            
        }
        //public StatValueContainer(IEnumerable<StatsMod> initValues)
        //{
        //    _currentEffects = new List<StatsEffect>();
        //    _currentMods = new();
        //    foreach (var mod in initValues)
        //    {
        //        ApplyStatsMod(mod);
        //    }
        //    _currentValue = _initValue;
        //    _cachedValue = _initValue;
        //}
        //public StatValueContainer(StatsMod initValue)
        //{
        //    _currentEffects = new List<StatsEffect>();
        //    _currentMods = new();
        //    ApplyStatsMod(initValue);
        //    _currentValue = _initValue;
        //    _cachedValue = _initValue;
        //}

        public void UpdateInDelta(float deltaTime)
        {
            UpdateMods(deltaTime);
            UpdateEffects(deltaTime);
            if (!_setup) { _setup = true; } // starting mods and effects are applied and the component can now process effects 
        }

        #region mods
        private List<StatsMod> _currentMods;
        public void ApplyStatsMod(StatsMod mod)
        {
            _currentMods.Add(mod);
        }
        void UpdateMods(float d)
        {
            foreach (var mod in _currentMods.ToList())
            {
                if (mod.CheckCondition(this))
                {
                    _initValue += mod.GetInitValue;
                    var valueDelta = mod.GetMaxValue - _maxValue;
                    _maxValue += mod.GetMaxValue;
                    _currentValue += 
                    
                }
            }
        }
        //public void RemoveStatsMod(SerializedStatModConfig mod)
        //{
        //    if (_currentMods.Contains(mod)) _currentMods.Remove(mod);
        //}

        #endregion

        #region temporary effects
        private List<StatsEffect> _currentEffects;

        public void ApplyStatsEffect(StatsEffect eff)
        {
            _cachedValue = _currentValue;
            _currentValue = Mathf.Clamp(_currentValue + eff.InitialValue, _minValue, _maxValue);
            _currentEffects.Add(eff);
        }

        void UpdateEffects(float d)
        {
            foreach (var eff in _currentEffects.ToList())
            {
                if (!eff.CheckCondition(d))
                {
                    _currentEffects.Remove(eff); // has no duration or is expired
                }
                else
                {
                    _cachedValue = _currentValue;
                    _currentValue = Mathf.Clamp(_currentValue + eff.FrameDelta, _minValue, _maxValue);
                }
            }

        }

        #endregion
    }
}