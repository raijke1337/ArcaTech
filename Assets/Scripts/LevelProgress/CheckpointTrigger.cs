using System;
using Arcatech.Interactions;
using UnityEngine;

namespace Arcatech.SaveSystem
{
    public class CheckpointTrigger : PassiveInteractionHandlerBase, ISavedProgressItem
    {
        public override void OnInteractorEnter(IInteractor interactor)
        {
            // if (interactor.InteractionContext.EntityComponent.CompareTag("Player"))
            // {
            //     Debug.Log("Checkpoint found!");
            //     ReadItemState = true;
            //     LevelProgressManager.Instance.SavedItemAnnounce(this);
            // }
        }

        public override void OnInteractorExit(IInteractor interactor)
        {
        }

        public string SavedItemID => baseGameEntityComponent.EntityID;
        public bool ReadItemState { get; private set; }
        public void OnWriteItemState(bool state, LevelProgressManager manager)
        {
            if (state)
            {
                gameObject.SetActive(false);
            }
        }
    }
}