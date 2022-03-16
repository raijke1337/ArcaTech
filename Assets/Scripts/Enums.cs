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

//side
public enum Allegiance
{
    Ally,
    Enemy
}

// all used stats 
public enum StatType
{
    Health,
    Shield,
    HealthRegen,
    ShieldRegen,
    Heat,
    HeatRegen,
    MoveSpeed
}
public enum EnemyStatType
{
    AggroRange,
    EvadeRange
}