using Arcatech.Actions;
using Arcatech.Effects;
using System;
using UnityEngine;
namespace Arcatech.Triggers
{

    public class StatsEffect
    {

        public float InitialValue { get; set; }
        public BaseStatType StatType { get; }
        int hash;
        public SerializedActionResult OnApply { get; }
        public StatsEffect(SerializedStatsEffectConfig cfg)
        {
            InitialValue = cfg.InitialValue;
            StatType = cfg.ChangedStat;
            hash = cfg.Hash;

            OnApply = cfg.OnApplyResult;
            _totalDelta = cfg.OverTimeValue;
            _totalTime = _timeLeft = cfg.OverTimeValueDuration;
            hash += UnityEngine.Random.Range(0, 9999999);
        }

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

        public float FrameDelta
        {
            get
            {
                return _totalDelta / _totalTime * _lastDelta;
            }
        }

        public override string ToString()
        {
            return string.Concat(StatType, " change ", InitialValue," + ", _totalDelta, " over ", _timeLeft);
        }

    }

}