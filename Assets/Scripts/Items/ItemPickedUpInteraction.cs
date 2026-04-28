using System;
using Arcatech.Interactions;
using Arcatech.Managers;
using Arcatech.SaveSystem;
using Arcatech.Texts;
using Arcatech.Units;
using com.cyborgAssets.inspectorButtonPro;
using UnityEngine;

namespace Arcatech.Items
{
    public class ItemPickedUpInteraction : InteractionEventHandlerBase,ISavedProgressItem
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
                CollectibleUpdate();
            }
        }

        private void Start()
        {
            if (content != null)
            {
                SetBillboard(content.Description);
            }
        }

        #region  collectible

        public string SavedItemID => entity.EntityID;
        public bool ReadItemState { get; private set; }
        public void OnWriteItemState(bool state, LevelProgressManager manager)
        {
            if (state) gameObject.SetActive(false);
        }
        void CollectibleUpdate()
        {
            ReadItemState = true;
            LevelProgressManager.Instance.SavedItemAnnounce(this);
        }
        
        #endregion
    }
}