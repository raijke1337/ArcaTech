using System;
using Arcatech.Interactions;
using UnityEngine;

namespace Arcatech.SaveSystem
{
    [RequireComponent(typeof(BaseGameEntityComponent))]
    public class CheckpointTrigger : InteractionEffect, ISavedProgressItem
    {
        private BaseGameEntityComponent baseGameEntityComponent;
        public string SavedItemID
        {
            get
            {
                if (baseGameEntityComponent == null)
                {
                    baseGameEntityComponent = GetComponent<BaseGameEntityComponent>();
                }
                return baseGameEntityComponent.EntityID;
            }
        }
        public bool ReadItemState { get; private set; }
        public void OnWriteItemState(bool state, LevelProgressManager manager)
        {
            ReadItemState = state;
            if (state)
            {
                gameObject.SetActive(false);
            }
        }

        public override void Play(InteractionContext ctx)
        {
            if (ctx.Interactor == null) return; // on level load
            if (ctx.Interactor.Entity.CompareTag("Player"))
            {
                Debug.Log("Checkpoint found!");
                ReadItemState = true;
                LevelProgressManager.Instance.SavedItemAnnounce(this);
            }
        }
    }
}