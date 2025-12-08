using UnityEngine;

namespace Arcatech.Interactions
{
    /// <summary>
    /// this class handles the action logic when an interaction is triggered
    /// </summary>
    public abstract class InteractionHandlerBase : MonoBehaviour, IInteractionHandler
    {
        public abstract void DoInteraction(bool success, IInteractor interactor);
        public abstract void OnPlayerEnter();
        public abstract void OnPlayerExit();
    }
}

