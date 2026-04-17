using System;
using Arcatech.Interactions;
using Arcatech.Managers;
using Arcatech.Texts;
using Arcatech.Units;
using com.cyborgAssets.inspectorButtonPro;
using UnityEngine;

namespace Arcatech.Items
{
    public class ItemPickedUpInteraction : InteractionEventHandlerBase
    {
        [SerializeField] private ItemSO content;
        [SerializeField] private int count = 1;
        [SerializeField] private Transform billboard;
        [SerializeField] private DialoguePart pickupMessage;
        public void PutItem(ItemSO item)
        {
            content = item;
            if (item.Description!= null) SetBillboard(item.Description);
        }
        ItemSO TakeItem()
        {
            var r =  content;
            content = null; 
            if (billboard)  billboard.gameObject.SetActive(false);
            if (pickupMessage) GameInterfaceManager.Instance.HandleDialoguePart(pickupMessage,true);
            return r;
        }

        private void SetBillboard(Description description)
        {
            var picture = description.Picture;
            if (!picture || !billboard) return;
            billboard.gameObject.SetActive(true);
            var renderer = billboard.GetComponent<Renderer>();
            renderer.material.mainTexture = picture.texture;
        }

        public override void DoInteraction(bool success, IInteractor interactor)
        {
            if (!success) return;
            if (interactor.InteractionContext.EntityComponent
                .TryGetComponent(out EntityInventoryComponent component))
            {
                component.PickUpItem(TakeItem().BuildItem(interactor.InteractionContext.EntityComponent), count);;
            }
        }

        private void Start()
        {
            if (content != null)
            {
                SetBillboard(content.Description);
            }
        }
        
    }
}