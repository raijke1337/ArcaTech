using UnityEngine;

namespace Arcatech.Interactions
{
    public class InteractionContext
    {
        public IInteractor Interactor;
        public InteractableComponent Target;
        public Vector3 InteractionPoint;
        public InteractionState State;
        public bool WasLocked; // флаг блокировки управления игрока
    }
}