using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Stats
{
    [CreateAssetMenu(fileName = "New BaseStatsConfig", menuName = "Base Stats/StartingStats", order = 1)]
    public class BaseStatsConfig : ScriptableObjectID
    {
        /*
        [SerializeField] SerializedStatModConfig[] InitialStats;
        public List<StatsMod> ListMods { get; private set; } = new List<StatsMod>();
        public Dictionary<BaseStatType, StatValueContainer> BuildBaseStats
        {
            get
            {
                Dictionary<BaseStatType, StatValueContainer> dict = new();
                foreach (var stat in InitialStats)
                {
                    var built = stat.BuildMod;
                    ListMods.Add(built);

                    if (!dict.ContainsKey(built.GetStatType))
                    {
                        dict[built.GetStatType] = new StatValueContainer();
                    }
                    dict[built.GetStatType].AddStatsMod(built);
                }
                return dict;
            }
        }
        */

        [Serializable]
        public struct ResourceStart
        {
            public ResourceStatType stat;
            public float baseMax;      // Base max capacity of the resource
            public bool setStartCurrentAsPercentOfMax;
            [Range(0f, 1f)] public float startPercent; // Used if above is true
            public float startCurrent; // Used if above is false
            public float minClampCurrent;     // Optional clamp for current
            public float maxClampCurrent;     // Optional clamp for current

        }

        [Tooltip("Define only the stats this unit should actually have.")]
        public List<ResourceStart> resources = new List<ResourceStart>();
    }
}
