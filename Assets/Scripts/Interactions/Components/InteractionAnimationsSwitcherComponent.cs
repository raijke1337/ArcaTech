using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Interactions
{
    public class InteractionAnimationsSwitcherComponent : MonoBehaviour
    {
        [SerializeField] private InteractionAnimationProfileSO profile;
        public InteractionAnimationProfileSO Profile => profile;

        public UnityEvent<InteractionState> StateAnimationExited = new();
    }
}
