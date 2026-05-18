using Arcatech.SaveSystem;
using Arcatech.Triggers;
using KBCore.Refs;
using NUnit.Framework;
using UnityEngine;

namespace Arcatech.Interactions
{
    /// <summary>
    /// defines how the interactable component activates
    /// </summary>
    [RequireComponent(typeof(InteractableComponent),typeof(SaveObjectID))]
    public abstract class InteractionTrigger : ValidatedMonoBehaviour, ITriggerNotificationReceiver,ISavedProgressItem
    {
        [SerializeField] protected TriggerTrackerComponent triggerTrackerComponent;
        [SerializeField,Self] protected InteractableComponent interactableComponent;
        [SerializeField,Self] protected SaveObjectID id;

        protected override void OnValidate()
        {
            base.OnValidate();
            Assert.IsNotNull(triggerTrackerComponent);
        }
        public abstract void TriggerEntered(TriggerHitInfo triggerHitInfo);
        public abstract void TriggerExited(TriggerHitInfo triggerExitInfo);
        
        protected virtual void Start()
        {
            triggerTrackerComponent.RegisterReceiver(this);
        }

        protected virtual void OnDisable()
        {
            triggerTrackerComponent.UnregisterReceiver(this);
        }


        #region saveload
        
        

        public string SavedItemID => id.UniqueId;
        public ProgressItemState ReadItemState { get; protected set; }
        public void ApplySaveState(ProgressItemState state, LevelProgressManager ctx)
        {
            switch (state)
            {
                case ProgressItemState.Default:
                    break;
                case ProgressItemState.Completed:
                    interactableComponent.gameObject.SetActive(false);
                    break;
            }
        }

        public string Name => gameObject.name;

        #endregion
    }
}