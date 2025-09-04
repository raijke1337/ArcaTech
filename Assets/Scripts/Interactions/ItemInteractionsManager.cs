using Arcatech.Level;
using Arcatech.Managers;
using Arcatech.Triggers;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Interactions
{
    public class ItemInteractionsManager : GenericLazySingleton<ItemInteractionsManager>
    {

        [Header("Detection")] [SerializeField] private float interactionRange = 3f;
        [SerializeField] private LayerMask interactionLayers = -1;
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        [Header("UI")] [SerializeField] private GameObject interactionPrompt;
        [SerializeField] private TMPro.TextMeshProUGUI promptText;

        private Camera playerCamera;
        private IInteractive currentInteractable;
        // private PlayerInventory playerInventory;

        // Events
        public UnityAction<IInteractive> OnInteractionAvailable = delegate { };
        public UnityAction OnInteractionUnavailable = delegate { };
        public UnityAction<IInteractive> OnInteractionPerformed = delegate { };

        void Update()
        {
            //DetectInteractables();
            HandleInput();
        }

        public void RegisterInteractor(IInteractor interactor)
        {
        }

        public void DeregisterInteractor(IInteractor interactor)
        {
        }
        
        void HandleInput()
        {
            /*if (Input.GetKeyDown(interactKey) && currentInteractable != null)
            {
                if (currentInteractable.CanInteract(gameObject))
                {
                    currentInteractable.Interact(gameObject);
                    OnInteractionPerformed?.Invoke(currentInteractable);
                }
            }*/
        }

        void UpdatePrompt(string text)
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
                if (promptText != null)
                    promptText.text = text;
            }
        }

        void HidePrompt()
        {
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }


    }
}

