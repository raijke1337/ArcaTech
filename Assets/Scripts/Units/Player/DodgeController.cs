using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class DodgeController : IStatsComponentForHandler
{

    Dictionary<DodgeStatType, StatValueContainer> _stats;
    public IReadOnlyDictionary<DodgeStatType,StatValueContainer> GetDodgeStats { get { return _stats; } }

    public int GetDodgeCharges()
    {
        return _stats != null ? (int)_stats[DodgeStatType.Charges].GetCurrent() : 0;
    }
    

    public DodgeController(string unitID)
    {
        _stats = new Dictionary<DodgeStatType, StatValueContainer>();
        var cfg = Extensions.GetAssetsFromPath<DodgeStatsConfig>(Constants.Configs.c_DodgeConfigsPath).First(t=>t.ID == unitID);
        foreach (var c in cfg.Stats)
        {
            _stats[c.Key] = new StatValueContainer(c.Value);
        }
        _timers = new List<Timer>();
    }

    public void SetupStatsComponent()
    {
        foreach (var st in _stats.Values)
        { st.Setup(); }
        
    }

    public bool IsDodgePossibleCheck()
    {
        if (_stats[DodgeStatType.Charges].GetCurrent() == 0f) return false;
        else
        {
            _stats[DodgeStatType.Charges].ChangeCurrent(-1);
            _timers.Add(new Timer(_stats[DodgeStatType.Cooldown].GetCurrent()));
            return true;
        }
    }

    private List<Timer> _timers;    // no coroutines

    public void UpdateInDelta(float deltaTime)
    {
        foreach (var t in _timers)
        {
            if (t.time <= 0f)
            {
                _stats[DodgeStatType.Charges].ChangeCurrent(1);
                _timers.Remove(t);
                return;
            }
            else
            {
                t.time -= deltaTime;
            }
        }

    }


}



