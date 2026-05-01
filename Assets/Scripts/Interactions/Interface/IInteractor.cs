using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Interactions
{
    /// <summary>
    /// some component that will do the interaction
    /// </summary>
    public interface IInteractor
    {
        void SetInteractionLock(bool locked);
        void RegisterInteractive(InteractableComponent interactive);
        void UnregisterInteractive(InteractableComponent interactive);
        InteractionState State { get; }
        
    }
}