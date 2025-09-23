
using Arcatech.Triggers;
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
        public float GetCurrent { get => Mathf.RoundToInt(_currentValue); }
        public float GetMax { get => _maxValue; }
        public float GetMin { get => _minValue; }
        public float GetPercent { get => _currentValue / _maxValue; }
        //public float CachedValue { get => _cachedValue; } // to store changes between updates
        public float GetFrameDeltaValue { get => Mathf.RoundToInt(_currentValue - _cachedValue); }
        public float GetFrameDeltaPercentAbs { get => Mathf.Abs((_currentValue - _cachedValue)/_maxValue); }
        #endregion

        [SerializeField] private float _currentValue;
        private float _cachedValue;
        [SerializeField] private float _maxValue;
        [SerializeField] private float _minValue = 0f;
        private float _initValue = 0f;
        private bool _setup = false;



        public void ResetMods()
        {
            _inactiveMods.Clear();
            _activeMods.Clear();
            _maxValue = 0f;
            _currentValue= 0f;
            _cachedValue = 0f;
            _minValue = 0f;
            _initValue = 0f;
            _setup = false;
        }

        public override string ToString()
        {
            return ($"{Mathf.RoundToInt(GetCurrent)} / {Mathf.RoundToInt(GetMax)}");
        }
        public StatValueContainer()
        {
            _currentEffects = new();
            _inactiveMods = new();
            _activeMods = new();
        }

        public void UpdateInDelta(float deltaTime)
        {
            if (_setup)
            {
                _cachedValue = _currentValue;
                UpdateMods(deltaTime);
                UpdateEffects(deltaTime);
            }
            else
            {
                UpdateMods(deltaTime);
                if (_activeMods.Count > 0 || _inactiveMods.Count > 0) 
                { 
                    _currentValue = _initValue;
                    _cachedValue = _initValue;
                    _setup = true; 
                }
            } // starting mods  are applied and the component can now process effects 
        }

        #region mods
        private List<StatsMod> _inactiveMods;
        private List<StatsMod> _activeMods;
        public void AddStatsMod(StatsMod mod) => _inactiveMods.Add(mod);

        public void RemoveStatMod(StatsMod mod)
        {
            if (_activeMods.Contains(mod))
            {
                _activeMods.Remove(mod);
                Debug.Log($"Removed mod {mod.ID}");
            }
            if (_inactiveMods.Contains(mod))
            {
                _inactiveMods.Remove(mod);
                Debug.Log($"Removed mod {mod.ID}");
            }

            UpdateMods(Time.deltaTime);
        }

        void UpdateMods(float d)
        {
            foreach (var mod in _inactiveMods.ToList())
            {
                if (mod.CheckCondition(this))
                {
                    _activeMods.Add(mod);
                    _inactiveMods.Remove(mod);
                    _initValue += mod.GetInitValue;
                    _maxValue += mod.GetMaxValue; 
                }
            }
            foreach (var mod in _activeMods.ToList())
            {
                if (!mod.CheckCondition(this))
                {
                    _activeMods.Remove(mod);
                    _inactiveMods.Add(mod);
                    _initValue -= mod.GetInitValue;
                    _maxValue -= mod.GetMaxValue;
                }
            }
            foreach (var mod in _activeMods)
            {
                float deltaChange =  mod.GetPerSecValue * d;
                _currentValue = Math.Clamp(_currentValue+ deltaChange, _minValue, _maxValue);
            }
        }

        #endregion

        #region temporary effects
        private List<StatsEffect> _currentEffects;

        public void ApplyStatsEffect(StatsEffect eff) =>_currentEffects.Add(eff);
        

        void UpdateEffects(float d)
        {
            foreach (var eff in _currentEffects.ToList())
            {
                if (!eff.InitDone)
                {
                    _currentValue = Mathf.Clamp(_currentValue + eff.InitialValue, _minValue, _maxValue);
                    eff.InitDone = true;
                }

                if (!eff.CheckCondition(d))
                {
                    _currentEffects.Remove(eff); // has no duration or is expired
                }
                else
                {
                    _currentValue = Mathf.Clamp(_currentValue + eff.FrameDelta, _minValue, _maxValue);
                }
            }

        }

        #endregion
    }
}