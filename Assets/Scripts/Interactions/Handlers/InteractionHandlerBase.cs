using System;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Interactions
{
    /// <summary>
    /// this class handles the action logic when an interaction is triggered
    /// </summary>
    public abstract class InteractionHandlerBase : MonoBehaviour, IInteractionHandler
    {
        
        public abstract void DoInteraction(IInteractor interactor, IInteractive item, IInteractionContext context);
    }
}

