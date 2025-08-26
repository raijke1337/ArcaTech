using Arcatech.Items;
using Arcatech.Triggers;
using Arcatech.Units;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Stats
{
    [Serializable]
    public class UnitStatsControllerOLD : ManagedControllerBase
    {
        #region depreciated

        private Dictionary<BaseStatType, StatValueContainer> _stats;



        public override void ControllerUpdate(float delta)
        {
            foreach (var stat in _stats)
            {
                stat.Value.UpdateInDelta(delta);
            }
        }

        public UnitStatsControllerOLD(SerializedStatModConfig[] initialStatMods, BaseEntityOLD dummyUnit) : base(dummyUnit)
        {
            _stats = new Dictionary<BaseStatType, StatValueContainer>();
            AddMods(initialStatMods);
        }
        public UnitStatsControllerOLD AddMods(SerializedStatModConfig[] mods)
        {
            foreach (var cfg in mods)
            {
                if (!_stats.ContainsKey(cfg.GetStatType))
                {
                    _stats[cfg.GetStatType] = new StatValueContainer(cfg);
                }
                else
                {
                    _stats[cfg.GetStatType].ApplyStatsMod(cfg);
                }
            }
            return this;
        }
        #endregion

        public bool CanApplyEffect (StatsEffect eff,IEquippable withShield = null)
        {
            StatValueContainer c;
            switch (eff.StatType)
            {
                case BaseStatType.Health:
                    if (withShield != null && eff.InitialValue < 0)
                    {
                        var shield = withShield as Shield;
                        var results = shield.AbsorbStrategy.SplitDamage(eff, _stats[BaseStatType.Energy]);
                        foreach (var result in results)
                        {
                            CanApplyEffect(result, null);
                        }
                        shield.AbsorbStrategy.OnApplicationResult.ProduceResult(Owner, Owner, Owner.transform);

                        return true;
                    }
                    else
                    {
                        if (_stats.TryGetValue(eff.StatType, out c))
                        {
                            c.ApplyStatsEffect(eff);
                            if (eff.OnApply!=null)
                            {
                                eff.OnApply.BuildActionResult().ProduceResult(Owner, Owner, Owner.transform);
                            }
                            return true;
                        }
                    }
                    break;
                default:
                    if (_stats.TryGetValue(eff.StatType, out c))
                    {
                        c.ApplyStatsEffect(eff);
                        return true;
                    }
                    break;
            }
            Debug.Log($"Can't apply effect {eff}, something went wrong");
            return false;
        }
        public bool CanApplyCost (StatsEffect cost)
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
        public void ApplyCost (StatsEffect cost)
        {
            var cont = _stats[cost.StatType];
            if ( cont.GetCurrent >= Mathf.Abs(cost.InitialValue))
            {
                cont.ApplyStatsEffect(cost);
            }
            else
            {
                Debug.LogError($"tried to apply cost {cost} in {Owner} without checking if its possible");
            }
        }

        public IReadOnlyDictionary<BaseStatType, StatValueContainer> GetStatValues => _stats;
        public bool TryGetStatValue(BaseStatType type,out StatValueContainer container)
        {
            container = null;
            if (_stats.ContainsKey(type))
            {
                container = _stats[type];
                return true;
            }
            else return false;
        }
        #region managed

        public override void StartController()
        {

        }




        public override void StopController()
        {
        }

        public override void FixedControllerUpdate(float fixedDelta)
        {
            
        }
        #endregion




    }
}