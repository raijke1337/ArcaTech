using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Stats
{
    [CreateAssetMenu(fileName = "baseStats_", menuName = "Game/Starting unit stats", order = 1)]
    public class BaseStatsConfig : ScriptableObjectID,IEquipmentStatsProvider
    {
        [SerializeField] private List<StatModifier> modifiers;
        [SerializeField] private List<PeriodicDelta> deltas;

        public IEnumerable<StatModifier> GetPersistentModifiers() => modifiers;
        public IEnumerable<PeriodicDelta> GetPeriodicDeltas()  => deltas;

        public BaseGameEntityComponent Source => null;
    }
}
