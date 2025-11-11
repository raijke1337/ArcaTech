using Arcatech.Items;
using Arcatech.Managers;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Actions
{
    [CreateAssetMenu(fileName = "New item is added to invenory", menuName = "Actions/Action Result/Item Is added to using inventory")]
    public class SerializedAddItemToInventoryResult : SerializedActionResult
    {
        [SerializeField] ItemSO itemToAdd;
        [SerializeField] int amountToAdd = 1;
        public override ActionResult BuildActionResult()
        {
            return new AddItemToInventoryResult(itemToAdd, amountToAdd);
        }
    }

    public class AddItemToInventoryResult : ActionResult
    {
        ItemSO itemToAdd;
        int amountToAdd;

        public AddItemToInventoryResult(ItemSO itemToAdd, int amountToAdd)
        {
            this.itemToAdd = itemToAdd;
            this.amountToAdd = amountToAdd;
        }



        public override bool ProduceResult(BaseGameEntityComponent user, BaseGameEntityComponent target, Transform place)
        {
            if (user.TryGetComponent<EntityInventoryComponent>(out var inv))
            {
                inv.PickUpItem(DataManager.Instance.MakeItem(itemToAdd,user));
                return true;
            }

            return false;
        }
    }
}