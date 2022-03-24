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
using RotaryHeart.Lib.SerializableDictionary;

[CreateAssetMenu(fileName = "New BaseStatsConfig", menuName = "Configurations/Stats", order = 1)]

public class BaseStatsConfig : ScriptableObject
{
    public string ID;
    public SerializableDictionaryBase<StatType, StatValueContainer> Stats;
}
