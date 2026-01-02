using System;
using Arcatech.Items;
using Arcatech.Stats;
using Arcatech.Texts;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Usables
{
    [CreateAssetMenu(fileName = "usable_", menuName = "Usables/Base Strategy")]
    public class SerializedUsableStrategy : ScriptableObject
    {
        
        [SerializeField] public Description description;
        public SharedUsablesSettings settings;
        
        [SerializeField] public SerializedStateTransition useStateTransition;

        [Header("Effects of usable")]
        public CompositeUsableApplicationSerialized[] compositeUsableEffects;

        public UsableStrategy Deserialize(BaseGameEntityComponent owner, EquipmentComponent ingameItem)
        {
            return new UsableStrategy(this,  owner, ingameItem);
        }
    }
    [Serializable]
    public struct SharedUsablesSettings
    {
        [SerializeField] public SerializedChargesStrategy charge;
        [SerializeField] public AppliedStatsDeltaEffect useCost;
        [SerializeField] public DrawItemsStrategy drawItemsStrategy;
    }
}

