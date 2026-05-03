using System;
using Arcatech.Items;
using Arcatech.Units;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Arcatech.Interactions
{
   // [CreateAssetMenu(fileName = "condition_hasItem_",menuName = "Interactions/Condition/Item in inventory")]
    public class ItemInInventoryInteractionCondition : InteractionCondition
    {
        [SerializeField, UnityEngine.Range(0, 10)] private int itemsConsumed = 0;
        [SerializeField] ItemSO itemNeeded;

        private void OnValidate()
        {
            Assert.IsNotNull(itemNeeded);
        }

        public override bool Check(InteractionContext context)
        {
            if (context.Interactor.Entity.TryGetComponent(out EntityInventoryComponent inventory))
            {
                return inventory.TryUseItem(itemNeeded,itemsConsumed);
            }
            return false;
        }
    }
}