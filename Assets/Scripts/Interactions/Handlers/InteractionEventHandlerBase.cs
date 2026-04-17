using UnityEngine;

namespace Arcatech.Interactions
{
    /// <summary>
    /// this class handles the action logic when an interaction is triggered
    /// </summary>
    public abstract class InteractionEventHandlerBase : MonoBehaviour, IActiveInteractionHandler
    {
        public abstract void DoInteraction(bool success, IInteractor interactor);
    }
}

