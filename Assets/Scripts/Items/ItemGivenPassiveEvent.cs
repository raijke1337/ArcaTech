using Arcatech.Interactions;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Items
{
    public class ItemGivenPassiveEvent : PassiveInteractionHandlerBase
    {
        [SerializeField] private ItemSO content;
        [SerializeField] private int count = 1;
        public override void OnInteractorEnter(IInteractor interactor)
        {
            if (interactor.InteractionContext.EntityComponent
                .TryGetComponent(out EntityInventoryComponent component))
            {
                component.PickUpItem(content.BuildItem(interactor.InteractionContext.EntityComponent), count);;
            }
        }

        public override void OnInteractorExit(IInteractor interactor)
        {
        }
    }
}