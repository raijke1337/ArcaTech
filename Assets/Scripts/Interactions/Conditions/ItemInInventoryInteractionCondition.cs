using Arcatech.Items;
using Arcatech.Units;
using NUnit.Framework;
using UnityEngine;

namespace Arcatech.Interactions
{
    [CreateAssetMenu(fileName = "New item in inventory condition",menuName = "Interactions/Condition/Item in inventory")]
    public class ItemInInventoryInteractionCondition : InteractionCondition
    {
        [SerializeField, UnityEngine.Range(0, 10)] private int itemsConsumed = 0;
        [SerializeField] ItemSO itemNeeded;

        private void OnValidate()
        {
            Assert.IsNotNull(itemNeeded);
            Debug.Log(itemNeeded.ID);
        }

        public override bool CheckCondition(IInteractor actor, IInteractive item, IInteractionContext context)
        {
            if (context.ActiveGameUnitComponent.TryGetComponent<EntityInventoryComponent>(out var inv))
            {
                return inv.TryUseItem(itemNeeded,itemsConsumed);
            }

            return false;
        }
    }
}