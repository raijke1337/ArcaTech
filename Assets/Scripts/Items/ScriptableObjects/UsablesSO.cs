using System;
using Arcatech.Items;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Arcatech.Usables
{
    /// <summary>
    ///  a weapon is a type of equipment that also has a weapon use strategy - melee or ranged (also an IUsable)
    /// </summary>
    [Serializable, CreateAssetMenu(fileName = "usable_", menuName = "Items/UsablesItem")]
    public class UsablesSO : EquipSO
    {
        [Header("Usables")] public SerializedDictionary<UnitActionType, SerializedUsableStrategy> usedActions;
        public override Item BuildItem(BaseGameEntityComponent owner)
        {
            return new UsablesItem(this, owner);
        }
        
    }
}