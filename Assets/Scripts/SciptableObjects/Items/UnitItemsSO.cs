using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
namespace Arcatech.Items
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Unit Items Preset", menuName = "Items/Inventory preset", order = 3)]
    public class UnitItemsSO : ScriptableObjectID
    {
        [SerializeField] public List<EquipSO> Equipment;
        [SerializeField, Space] public List<ItemSO> Inventory;

        private void OnValidate()
        {
            foreach (var item in Inventory)
            {
                Assert.IsNotNull(item);
            }
            foreach (var item in Equipment)
            {
                Assert.IsNotNull(item);
            }
        }

        public UnitInventoryItemConfigsContainer BuildContainer()
        {
            return new UnitInventoryItemConfigsContainer(this);
        }
    }
}