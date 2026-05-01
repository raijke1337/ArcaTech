using UnityEngine;

namespace Arcatech.Interactions
{
    public class InteractionContext
    {
        public IInteractor Interactor;
        public InteractableComponent Target;
        public Vector3 InteractionPoint;
        public InteractionStatus FinalStatus;
        public bool WasLocked; // флаг блокировки управления игрока
    }
}