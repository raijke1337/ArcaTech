using Arcatech.Triggers;
using Arcatech.Units;
using System;
using Arcatech.Items;
using Arcatech.Stats;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Usables
{
    /// <summary>
    ///  a weapon is a type of equipment that also has a weapon use strategy - melee or ranged (also an IUsable)
    /// </summary>
    [Serializable, CreateAssetMenu(fileName = "New Usables Item", menuName = "Items/UsablesItem")]
    public class UsablesSO : EquipSO
    {
        [Header("Usables")] public SerializedDictionary<UnitActionType, SerializedUsableStrategy> usedActions;
        public override Item BuildItem(BaseGameEntityComponent owner)
        {
            return new UsablesItem(this, owner);
        }
        
    }
}