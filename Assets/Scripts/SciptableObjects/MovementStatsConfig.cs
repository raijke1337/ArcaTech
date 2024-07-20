﻿
using Arcatech.Stats;
using Arcatech.Triggers;
using AYellowpaper.SerializedCollections;
using System;
using UnityEngine;

namespace Arcatech.Units.Stats
{
    [CreateAssetMenu(fileName = "New MoveStatsConfig", menuName = "Configurations/Move Stats"),Serializable]
    public class MovementStatsConfig : ScriptableObjectID
    {
        public SerializedDictionary<MovementStatType, SimpleContainerConfig> Stats;
        public BaseUnitAction JumpAction;
    }



    [Serializable]
    public struct SimpleContainerConfig
    {
        public float Max;
        public float Min;
        public float Start;
    }
}