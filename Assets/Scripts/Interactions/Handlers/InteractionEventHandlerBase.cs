using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Interactions
{
    /// <summary>
    /// this class handles the action logic when an interaction is triggered
    /// </summary>
    [RequireComponent(typeof(BaseGameEntityComponent))]
    public abstract class InteractionEventHandlerBase : ValidatedMonoBehaviour, IActiveInteractionHandler
    {
        [SerializeField, Self] protected BaseGameEntityComponent entity;
        public abstract void DoInteraction(bool success, IInteractor interactor);
    }
}

