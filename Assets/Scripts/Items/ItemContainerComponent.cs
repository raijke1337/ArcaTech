using Arcatech.Interactions;
using UnityEngine;

namespace Arcatech.Items
{
    public class ItemContainerComponent : InteractionHandlerBase
    {
        [SerializeField] private ItemSO content;

        public void PackItem(ItemSO item)
        {
            content = item;
        }

        public override void DoInteraction(bool success, IInteractor interactor)
        {
            throw new System.NotImplementedException();
        }

        public override void OnPlayerEnter()
        {
            throw new System.NotImplementedException();
        }

        public override void OnPlayerExit()
        {
            throw new System.NotImplementedException();
        }
    }
}