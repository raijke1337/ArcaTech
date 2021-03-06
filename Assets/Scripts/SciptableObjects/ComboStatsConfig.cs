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

[CreateAssetMenu(fileName = "New HeatStatsConfig", menuName = "Configurations/ComboController")]
public class ComboStatsConfig : ScriptableObjectID
{
    [SerializeField] public StatValueContainer ComboContainer;
    public float DegenCoeff;
    public float HeatTimeout;
}

