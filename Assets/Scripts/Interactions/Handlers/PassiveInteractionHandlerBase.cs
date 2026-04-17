using UnityEngine;

namespace Arcatech.Interactions
{
    public abstract class PassiveInteractionHandlerBase : MonoBehaviour, IPassiveInteractionHandler
    {
        
        public abstract void OnInteractorEnter(IInteractor interactor);
        public abstract void OnInteractorExit(IInteractor interactor);
    }
}