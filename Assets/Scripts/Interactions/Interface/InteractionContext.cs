using UnityEngine;

namespace Arcatech.Interactions
{
    public class InteractionContext
    {
        public IInteractor Interactor;
        public BaseGameEntityComponent Target;
        public InteractionState State;

        /// <summary>
        /// can use in case pooling is implemented
        /// </summary>
        public void Reset()
        {
            Interactor = null;
            Target = null;
            State = InteractionState.Idle;
        }
    }
}