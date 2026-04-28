using Arcatech.Interactions;
using Arcatech.Triggers;
using KBCore.Refs;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Arcatech.SaveSystem
{
    public class SecretAreaTriggerComponent : PassiveInteractionHandlerBase, ISavedProgressItem
    {
        public string SavedItemID => baseGameEntityComponent.EntityID;
        public bool ReadItemState { get; private set; }
        public void OnWriteItemState(bool state, LevelProgressManager manager)
        {
            if (state) // zone found. Can disable the handler
            {
                gameObject.SetActive(false);
            }
        }

        public override void OnInteractorEnter(IInteractor interactor)
        {            
            if (interactor.InteractionContext.EntityComponent.CompareTag("Player"))
            {
                Debug.Log("Secret Area found!");
                ReadItemState = true;
                LevelProgressManager.Instance.SavedItemAnnounce(this);
            }
        }

        public override void OnInteractorExit(IInteractor interactor)
        {
        }
    }
}