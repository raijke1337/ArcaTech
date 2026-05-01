using System;
using System.Collections;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Interactions
{
    public class AnimationEventExecutor : InteractionExecutor, IStateAugmentor
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private string _stateName;
        [SerializeField] private float _safetyTimeout = 3f;

        private Action<InteractionState> _cb;

        public override void Execute(InteractionContext ctx, Action<InteractionState> onComplete)
        {
            _cb = onComplete;
            _animator.Play(_stateName);
            StartCoroutine(WaitRoutine());
        }

        private IEnumerator WaitRoutine()
        {
            yield return new WaitForSeconds(_safetyTimeout);
            _cb?.Invoke(InteractionState.Success);
            _cb = null;
        }

        // Вызывается из AnimationEvent в конце клипа
        public void NotifyAnimationFinished()
        {
            _cb?.Invoke(InteractionState.Success);
            _cb = null;
        }

        #region State Augmentor
        public void Attach(IStateAugmentorReceiver machine)
        {
            throw new NotImplementedException();
        }

        public void Detach(IStateAugmentorReceiver machine)
        {
            throw new NotImplementedException();
        }

        public void OnStateEntered(UnitState state, StateMachineContext context)
        {
            throw new NotImplementedException();
        }

        public void OnStateExited(UnitState state, StateMachineContext context)
        {
            throw new NotImplementedException();
        }
        
        #endregion
    }
}