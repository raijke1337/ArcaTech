
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

       // private bool _setup = false;

        public override string ToString()
        {
            return ($"{Mathf.RoundToInt(GetCurrent)} / {Mathf.RoundToInt(GetMax)}");
        }

        public StatValueContainer(IEnumerable<SerializedStatModConfig> initValues)
        {
            _currentEffects = new List<StatsEffect>();
            _currentMods = new();
            foreach (var mod in initValues)
            {
                ApplyStatsMod(mod);
            }
            _currentValue = _initValue;
            _cachedValue = _initValue;
        }
        public StatValueContainer(SerializedStatModConfig initValue)
        {
            _currentEffects = new List<StatsEffect>();
            _currentMods = new();
            ApplyStatsMod(initValue);
            _currentValue = _initValue;
            _cachedValue = _initValue;
        }

        public void UpdateInDelta(float deltaTime)
        {
            UpdateMods(deltaTime);
            UpdateEffects(deltaTime);
        }

        #region mods
        private List<SerializedStatModConfig> _currentMods;
        public void ApplyStatsMod(SerializedStatModConfig mod)
        {
            _maxValue += mod.GetMaxValue;
            _initValue += mod.GetInitValue;
            _currentMods.Add(mod);
        }
        void UpdateMods(float d)
        {
            foreach (var mod in _currentMods.ToList())
            {
                if (mod.CheckCondition(this))
                {
                    _cachedValue = _currentValue;
                    _currentValue = Mathf.Clamp(_currentValue + (mod.GetPerSecValue * d), _minValue, _maxValue);
                }
                else
                {
                   // Debug.Log($"Condition not met for {mod}");
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