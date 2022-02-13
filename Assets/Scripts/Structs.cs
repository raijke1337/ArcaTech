using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using RotaryHeart;
using RotaryHeart.Lib.SerializableDictionary;

public delegate void SimpleEventsHandler();
public delegate void SimpleEventsHandler<T>(T arg);


public enum Allegiance
{
    Ally,
    Enemy
}

public class MovementDebugData
{
    public Vector3 _facing;
    public Vector3 _movement;
    public Vector3 _animVector;
}


[Serializable]
public class StatContainer
{
    /// <summary>
    /// keeps the current modifiers to a stat and its maximum value
    /// </summary>
    private LinkedList<StatModData> _statMods = new LinkedList<StatModData>();

    [SerializeField]
    private float _defaultValue;

    public StatRange Range;

    public float GetCurrentValue { get; private set; }

    public void UpdateStatValue()
    {
        // gets the sum of all mods in the list
        if (_statMods == null)
        {
            GetCurrentValue = _defaultValue;
        }
        else
        {
            GetCurrentValue = _defaultValue + _statMods.Sum(t => t.Value);
        }         
    }
    public void AddStatMod(StatModData data)
    {
        _statMods.AddLast(data);
        UpdateStatValue();
    }
    public void RemoveStatMod(string ID)
    {
        var stat = _statMods.First(t => t.ID == ID);
        _statMods.Remove(stat);
        UpdateStatValue();
    }
    public StatContainer(StatRange range)
    {
        Range.Max = range.Max;
        Range.Min = range.Min;
        _defaultValue = range.Max;
        Debug.Log("Test");
    }

}
//this is used by stat handler
[Serializable]
public struct StatModData
{
    public StatType Type;
    public float Value;
    public string ID;
    public StatModData(StatType type, float value, string id)
    {
        Type = type; Value = value; ID = id;
    }
}
[Serializable]
public struct StatRange
{
    [SerializeField]
    public float Min;
    [SerializeField]
    public float Max;
    public StatRange(float min, float max)
    {
        Min = min; Max = max;
    }
    public override string ToString()
    {
        return string.Concat($"Min: {Min} Max: {Max}");
    }
    public override bool Equals(object obj)
    {
        if (!(obj is StatRange)) return false;
        var _range = (StatRange)obj;
        return _range.Max == Max && _range.Min == Min;
    }
    public override int GetHashCode()
    {
        var hashCode = -1998946679;
        hashCode = hashCode * -1521134295 + Min.GetHashCode();
        hashCode = hashCode * -1521134295 + Max.GetHashCode();
        return hashCode;
    }
}


public enum StatType
{
    Health,
    Shield,
    MoveSpeed,
    CritMult,
    DashRange,
    Heat,
    HealthRegen,
    ShieldRegen,
    HeatRegen
}


// this is applied by weapons items etc
[Serializable]
public struct EffectData
{
    public float Duration;
    public string ID;
    public Sprite Sprite;

    public EffectData(float dur, string id, Sprite sp)
    {
        Duration = dur; ID = id; Sprite = sp;
    }
}


[Serializable]
public class StatsDictionary : SerializableDictionaryBase<StatType,StatContainer>{}
