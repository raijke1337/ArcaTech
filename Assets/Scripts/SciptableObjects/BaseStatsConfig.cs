using Arcatech.Triggers;
using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Units.Stats
{
    [CreateAssetMenu(fileName = "New BaseStatsConfig", menuName = "Units/Base Stats")]
    public class BaseStatsConfig : ScriptableObjectID
    {
        public string DisplayName;
        public SerializedStatModConfig[] InitialStats;
    }


}