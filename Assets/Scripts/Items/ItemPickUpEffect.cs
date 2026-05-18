using System;
using Arcatech.Interactions;
using Arcatech.Managers;
using Arcatech.SaveSystem;
using Arcatech.Texts;
using Arcatech.Units;
using com.cyborgAssets.inspectorButtonPro;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Items
{
    [RequireComponent(typeof(BaseGameEntityComponent))]
    public class ItemPickUpEffect : InteractionEffect
    {
        private BaseGameEntityComponent _entity;
        [SerializeField] private ItemSO content;
        [SerializeField] private int count = 1;
        [SerializeField] private Transform billboard;
        
        
        #region intraction
        
        public override void Play(InteractionContext ctx)
        {
            if (ctx.Interactor.Entity
                .TryGetComponent(out EntityInventoryComponent component))
            {
                component.PickUpItem(TakeItem().BuildItem(ctx.Interactor.Entity), count);;
            }
        }

        public override void OnLoadLevelState(ProgressItemState stateToLoad)
        {
            // noop
        }

        #endregion
        
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

        private void Start()
        {
            if (content != null)
            {
                SetBillboard(content.Description);
            }
        }

    }
}