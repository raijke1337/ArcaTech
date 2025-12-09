using Arcatech.Interactions;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Items
{
    public class ItemContainerComponent : InteractionHandlerBase
    {
        [SerializeField] private ItemSO content;
        
        public void PutItem(ItemSO item)=>content = item;
        ItemSO TakeItem()
        {
            ItemSO r =  content;
            content = null; 
            return r;
        }

        public override void DoInteraction(bool success, IInteractor interactor)
        {
            if (!success) return;
            if (interactor.InteractionContext.EntityComponent
                .TryGetComponent(out EntityInventoryComponent component))
            {
                component.PickUpItem(TakeItem().BuildItem(interactor.InteractionContext.EntityComponent));
            }
        }

        public override void OnPlayerEnter()
        { }

        public override void OnPlayerExit()
        { }
        
        
    }
}