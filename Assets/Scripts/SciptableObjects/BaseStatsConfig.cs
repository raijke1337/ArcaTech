using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Stats
{
    [CreateAssetMenu(fileName = "New BaseStatsConfig", menuName = "Units/Base Stats")]
    public class BaseStatsConfig : ScriptableObjectID
    {
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




    }
}
