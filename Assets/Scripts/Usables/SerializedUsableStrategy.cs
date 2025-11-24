using System;
using Arcatech.Actions;
using Arcatech.Items;
using Arcatech.Stats;
using Arcatech.Texts;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Usables
{
    [CreateAssetMenu(fileName = "New Usable Strategy", menuName = "Usables/Base Strategy")]
    public class SerializedUsableStrategy : ScriptableObject
    {
        
        [SerializeField] public Description description;
        public SharedUsablesSettings settings;
        
        [SerializeField] public SerializedStateTransition useStateTransition;
        
        [Header("Hit producer")]
        public SerializedHitProducer hitProducer;
        
        [Header("Apply effects of hits")]
        public SerializedEffectApplier effectApplier;
        
        [Header("The effects")]
        public SerializedActionResult[] effects;

        public UsableStrategy Deserialize(BaseGameEntityComponent owner, EquipmentComponent ingameItem)
        {
            return new UsableStrategy(this,  owner, ingameItem);
        }
    }
    [Serializable]
    public struct SharedUsablesSettings
    {
        [SerializeField] public int charges;
        [SerializeField] public float chargeReload;
        [SerializeField] public StatsEffect useCost;
        [SerializeField] public DrawItemsStrategy drawItemsStrategy;
    }
}