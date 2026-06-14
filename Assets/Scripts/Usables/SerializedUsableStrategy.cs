using System;
using Arcatech.Items;
using Arcatech.Stats;
using Arcatech.Texts;
using Arcatech.Units;
using Arcatech.Usables.Effects;
using UnityEngine;

namespace Arcatech.Usables
{
    [CreateAssetMenu(fileName = "usable_", menuName = "Usables/Base Strategy")]
    public class SerializedUsableStrategy : ScriptableObject
    {
        
        public Description description;
        public GenericUsableConfig settings;
        public SerializedStateTransition useStateTransition;

        [Header("Effects of usable")]
        public UsableDataContainer[] usableData;

        public UsableStrategy Deserialize(BaseGameEntityComponent owner, EquipmentComponent ingameItem)
        {
            return new UsableStrategy(this,  owner, ingameItem);
        }
    }
    [Serializable]
    public struct GenericUsableConfig
    {
        [SerializeField] public SerializedGenericCooldownStrategy charge;
        [SerializeField] public AppliedStatsDeltaEffect useCost;
        [SerializeField] public DrawItemsStrategy drawItemsStrategy;
    }
}

