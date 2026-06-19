using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Arcatech.Stats
{
    [CreateAssetMenu(fileName = "baseStats_", menuName = "Game/Starting unit stats", order = 1)]
    public class BaseStatsConfig : ScriptableObjectID//,IEquipmentStatsProvider
    {
        // [SerializeField] private List<StatModifier> modifiers;
        // [SerializeField] private List<PeriodicDelta> deltas;
        //
        // public IEnumerable<StatModifier> GetPersistentModifiers() => modifiers;
        // public IEnumerable<PeriodicDelta> GetPeriodicDeltas()  => deltas;
        //
        // public BaseGameEntityComponent Source => null;

        public SerializedDictionary<ResourceStatType, UnitResource> resources;
    }

    [Serializable]
    public struct UnitResource
    {
        public int baseMax;
        public int startCurrent;
        public int minClampCurrent;
        public int maxClampCurrent;
        
        public bool setStartCurrentAsPercentOfMax;
        [Range(0,1)]public float startPercent;
    }
}
