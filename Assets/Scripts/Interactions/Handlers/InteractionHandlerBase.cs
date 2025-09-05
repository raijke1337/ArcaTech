using System.Collections.Generic;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Interactions
{
    public abstract class InteractionHandlerBase : ValidatedMonoBehaviour, IInteractionHandler
    {
        
        public abstract void DoInteraction(IInteractor interactor, IInteractive item, IInteractionContext context);
    }
}

