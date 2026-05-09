using System.Collections;
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

        
        private bool _blockingComplete;
        private string _lastPlayedState;
        private Coroutine _waitCoroutine;
        // ─── Blocking API ───
        public override bool IsBlocking => !string.IsNullOrEmpty(_lastPlayedState);
        public override bool IsBlockingComplete => _blockingComplete;
        
        private void Awake()
        {
            _animator = GetComponent<Animator>();
            if (interactorPlace ==  null) interactorPlace = transform;
        }
        public override void OnCancelled()
        {
            StopWaiting();
            _blockingComplete = true;
        }
        public override void Play(InteractionContext ctx)
        {
            _blockingComplete = false;
            _lastPlayedState = null;
            StopWaiting();

            // Ставим игрока в точку (с защитой от null для автоактивации)
            if (interactorPlace != null && ctx.Interactor?.Entity != null)
            {
                ctx.Interactor.Entity.transform.SetPositionAndRotation(
                    interactorPlace.position, interactorPlace.rotation);
            }

            if (_animator != null && _states.TryGetValue(ctx.State, out var anim))
            {
                _lastPlayedState = anim;
                _animator.Play(anim);
                _waitCoroutine = StartCoroutine(WaitForCompletion(anim));
            }
            else
            {
                _blockingComplete = true; // нет анимации для этого состояния — мгновенно
            }
        }
        
        private IEnumerator WaitForCompletion(string stateName)
        {
            // Даём Animator один кадр, чтобы переключиться
            yield return null;

            // Fallback: ждём конца клипа по normalizedTime 
            // (работает, если нет переходов и вызовете NotifyAnimationFinished для надёжности)
            while (_animator != null && 
                   _animator.GetCurrentAnimatorStateInfo(0).IsName(stateName) &&
                   _animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
            {
                yield return null;
            }

            _blockingComplete = true;
        }
        private void StopWaiting()
        {
            if (_waitCoroutine != null)
            {
                StopCoroutine(_waitCoroutine);
                _waitCoroutine = null;
            }
        }
        public void ANIM_NotifyAnimationFinished()
        {
            _blockingComplete = true;
        }
        
        private void OnDrawGizmos()
        {
            if (interactorPlace == null) return;

            Gizmos.color = Color.green;
            
            // Draw a wire cube (rectangle) to represent the interactor's body placement
            Gizmos.DrawWireCube(interactorPlace.position, new Vector3(0.5f, 1.8f, 0.5f)); // Approximate human proportions: width 0.5, height 1.8, depth 0.5
            
            // Draw a ray for the line of sight (forward direction)
            Gizmos.DrawRay(interactorPlace.position, interactorPlace.forward * 0.5f); // Forward ray of length 2 units
            
            // Optional: Add a small sphere at the forward endpoint for clarity
            Gizmos.DrawSphere(interactorPlace.position + interactorPlace.forward * 0.5f, 0.1f);
        }
    }
}