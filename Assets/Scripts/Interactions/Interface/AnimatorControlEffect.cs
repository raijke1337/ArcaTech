using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Arcatech.Interactions
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorControlEffect : InteractionEffect
    {
        [SerializeField] SerializedDictionary<InteractionState,string> _states;
        [SerializeField] private Transform interactorPlace;
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            if (interactorPlace ==  null) interactorPlace = transform;
        }

        public override void Play(InteractionContext ctx)
        {
            ctx.Interactor.Entity.transform.SetPositionAndRotation(interactorPlace.position, interactorPlace.rotation);
            if (_states.TryGetValue(ctx.State, out var anim)) _animator.Play(anim);
        }
        
        private void OnDrawGizmos()
        {
            if (interactorPlace == null) return;

            Gizmos.color = Color.green;
            
            // Draw a wire cube (rectangle) to represent the interactor's body placement
            Gizmos.DrawWireCube(interactorPlace.position, new Vector3(0.5f, 1.8f, 0.5f)); // Approximate human proportions: width 0.5, height 1.8, depth 0.5
            
            // Draw a ray for the line of sight (forward direction)
            Gizmos.DrawRay(interactorPlace.position, interactorPlace.forward * 2f); // Forward ray of length 2 units
            
            // Optional: Add a small sphere at the forward endpoint for clarity
            Gizmos.DrawSphere(interactorPlace.position + interactorPlace.forward * 2f, 0.1f);
        }
    }
}