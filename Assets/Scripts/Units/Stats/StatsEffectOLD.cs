using System;
using Arcatech.Actions;

namespace Arcatech.Stats
{

    public class StatsEffectOLD
    {/*
        public bool InitDone { get; set; } = false;

        public float InitialValue { get => _initial; }
        public float OverTimeValue { get => _totalDelta; }
        public float OverTimeDuration { get => _totalTime; }
        public ResourceStatType StatType { get; }

        public SerializedActionResult OnApply { get; }


        public static StatsEffectOLD BuildEffect(SerializedStatsEffect c) => new (c);
        private StatsEffectOLD(SerializedStatsEffect cfg)
        {
            _initial = cfg.InitialValue;
            StatType = cfg.ChangedStat;

            OnApply = cfg.OnApplyResult;
            _totalDelta = cfg.OverTimeValue;
            _totalTime = _timeLeft = cfg.OverTimeValueDuration;
        }
        

        float _initial;
        float _timeLeft;
        float _totalTime;
        float _totalDelta;
        float _lastDelta;
        public bool CheckCondition(float deltaTime)
        {
            _timeLeft -= deltaTime;
            _lastDelta = deltaTime;
            return _timeLeft > 0;
        }

        public float FrameDelta => _totalDelta / _totalTime * _lastDelta;
        public override string ToString()
        {
            return string.Concat(StatType, " change ", InitialValue," + ", _totalDelta, " over ", _timeLeft);
        }

        
        #region Icloneable
        
        private StatsEffectOLD(float initial, float timeLeft, float totalTime, float totalDelta, float lastDelta, ResourceStatType statType, SerializedActionResult onApply)
        {
            _initial = initial;
            _timeLeft = timeLeft;
            _totalTime = totalTime;
            _totalDelta = totalDelta;
            _lastDelta = lastDelta;
            StatType = statType;
            OnApply = onApply;
        }
        public object Clone()
        {
            return new StatsEffectOLD(_initial, _timeLeft, _totalTime, _totalDelta, _lastDelta, StatType, OnApply);
        }*/
       // #endregion
    }

}