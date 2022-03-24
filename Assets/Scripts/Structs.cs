using System;
using System.Collections.Generic;
using UnityEngine;

public delegate void SimpleEventsHandler();
public delegate void SimpleEventsHandler<T>(T arg);
public delegate void WeaponSwitchEventHandler(WeaponType type);

public static class Constants
{
    public const string c_TriggersConfigsPath = "/Scripts/Configurations/Triggers";
    public const string c_WeapConfigsPath = "/Scripts/Configurations/Weapons";
    public const string c_BaseStatConfigsPath = "/Scripts/Configurations/BaseStats";
    public const string c_ProjectileConfigsPath = "/Scripts/Configurations/Projectiles";
    public const string c_SkillClassesPath = "/Scripts/Weapons/Skills";
    public const string c_WeaponPrefabsPath = "/Prefabs/Weapons";
    public const float c_ProjectileTriggerActivateDelay = 0.15f; 
    // to not damage self with projectiles, obvious bandaid todo
}

#region interfaces

public interface IStatsComponentForHandler
{ 
    void UpdateInDelta(float deltaTime);
    void Setup();
}
public interface IStatsAddEffects
{
    void AddTriggeredEffect(TriggeredEffect effect);
    void AddTriggeredEffects(IEnumerable<TriggeredEffect> effects);
}

public interface IWeapon
{    
    bool UseWeapon();
    public GameObject GetObject();
    int GetAmmo();
    string GetRelatedSkillID();
}

#endregion
[Serializable] public class Timer { public float time; public Timer(float t) { time = t; } }


[Serializable]
public class StatValueContainer
{
    [SerializeField,] private float _start;
    [SerializeField] private float _max;
    [SerializeField] private float _min;
    [SerializeField] private float _current;

    public SimpleEventsHandler<float> ValueDecreasedEvent;

    public float GetCurrent() => _current;
    public float GetMax() => _max;
    public float GetMin() => _min;
    public float GetStart() => _start;
    /// <summary>
    /// adds the value
    /// </summary>
    /// <param name="value">how much to add or remove</param>
    public void ChangeCurrent(float value)
    {
        _current = Mathf.Clamp(_current+value,_min,_max);
        if (value < 0f)
        ValueDecreasedEvent?.Invoke(_current);
    }

    public void Setup()
    {
        _current = _start;
    }
    //todo
    public StatValueContainer(StatValueContainer preset)
    {
        _start = preset._start;
        _max = preset._max;
        _min = preset._min;
        Setup();
    }
}



