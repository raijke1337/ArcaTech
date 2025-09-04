using Arcatech.Actions;
using Arcatech.Stats;
using Arcatech.Triggers;
using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Items
{
    public class ShieldAbsorbStrategy : IShieldAbsorbStrategy
    {
        float _absorbPercent;
        float _absorbConversionToEnergyK;
        SerializedActionResult _result;
        float _absorbTime;

        public IActionResult OnApplicationResult { get => _result.BuildActionResult(); }

        public ShieldAbsorbStrategy(float mult, float coef, float splitTime, SerializedActionResult onApply)
        {
            this._absorbPercent = mult;
            this._absorbConversionToEnergyK = coef;
            _result = onApply;
            _absorbTime = splitTime;
        }
        public StatsEffect[] SplitDamage(StatsEffect damage, StatValueContainer shieldCharge)
        {
            List<StatsEffect> results = new();

            // instant dmg part
            float toCalc = damage.InitialValue * _absorbPercent;
            float notCalc = damage.InitialValue - toCalc;
            float absorbInstantCalcEnergy = toCalc * _absorbConversionToEnergyK;

            if (shieldCharge.GetCurrent >= Mathf.Abs(absorbInstantCalcEnergy))
            {
                results.Add(new StatsEffect(BaseStatType.Health, notCalc, 0, 0, damage.OnApply));
                if (_absorbTime == 0)
                {
                    results.Add(new StatsEffect(BaseStatType.Energy, absorbInstantCalcEnergy, 0, 0));
                    
                }
                else
                {
                    results.Add(new StatsEffect(BaseStatType.Energy, 0, absorbInstantCalcEnergy, _absorbTime));
                }
            }
            else
            {
                float add = absorbInstantCalcEnergy - shieldCharge.GetCurrent;
                results.Add(new StatsEffect(BaseStatType.Health, notCalc+add, 0, 0, damage.OnApply));
                results.Add(new StatsEffect(BaseStatType.Energy, absorbInstantCalcEnergy-add, 0, 0, _result));
            }


            if (damage.OverTimeValue > 0)
            {
                //do dot part calculation
                // just turn everything into energy drain
                if (shieldCharge.GetCurrent >= Mathf.Abs(absorbInstantCalcEnergy))
                {
                    results.Add(new StatsEffect(BaseStatType.Energy, 0, damage.OverTimeValue * _absorbConversionToEnergyK , damage.OverTimeDuration));
                }
                else
                // dot remains
                {
                    results.Add(new StatsEffect(damage.StatType, 0, damage.OverTimeValue, damage.OverTimeDuration));
                }
            }

            return results.ToArray();
        }
    }
}