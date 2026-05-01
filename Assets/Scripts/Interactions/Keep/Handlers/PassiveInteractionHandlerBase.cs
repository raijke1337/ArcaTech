using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Interactions
{    
    [RequireComponent(typeof(BaseGameEntityComponent))]
    public abstract class PassiveInteractionHandlerBase : ValidatedMonoBehaviour, IPassiveInteractionHandler
    {        
        [SerializeField,Self] protected BaseGameEntityComponent baseGameEntityComponent;
        public abstract void OnInteractorEnter(IInteractor interactor);
        public abstract void OnInteractorExit(IInteractor interactor);
    }
}