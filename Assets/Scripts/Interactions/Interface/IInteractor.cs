using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Interactions
{
    /// <summary>
    /// component that will do the interaction
    /// </summary>
    public interface IInteractor
    {
        void SetInteractionState(InteractionState state);
        void RegisterInteractive(InteractableComponent interactive);
        void UnregisterInteractive(InteractableComponent interactive);
        BaseGameEntityComponent Entity { get; }
        InteractionState State { get; }
    }
}