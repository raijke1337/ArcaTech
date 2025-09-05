using Arcatech.Items;
using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Units
{
    [CreateAssetMenu(fileName = "New Items placement IStrategy", menuName = "Items/Draw strategy")]
    public class DrawItemsStrategy : ScriptableObject, IDrawItemStrategy
    {
        public Dictionary<EquipmentType, ItemPlaceType> GetPlaces => _dict;

        [SerializeField] SerializedDictionary<EquipmentType, ItemPlaceType> _dict;
    }


}