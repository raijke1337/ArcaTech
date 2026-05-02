using System.Collections.Generic;
using Arcatech.Managers;
using Arcatech.SaveSystem;
using Arcatech.Texts;
using Arcatech.Triggers;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Arcatech.Interactions
{
    /// <summary>
    /// item to be interacted with
    /// </summary>
    [RequireComponent(typeof(BaseGameEntityComponent))]
    public class InteractableComponent : ValidatedMonoBehaviour
    {
        
        [SerializeField, Self] private BaseGameEntityComponent entity;
        [Header("Pipeline")] 
        [SerializeField,Self] protected InteractionTrigger trigger;
        [SerializeField] private List<InteractionCondition> conditions;
        [SerializeField] private List<InteractionEffect> preExecuteEffects;
        [SerializeField] private InteractionExecutor executor;
        [SerializeField] private List<InteractionEffect> duringExecuteEffects;

        [Header("Post Effects")] [SerializeField]
        private List<InteractionEffect> successEffects;
        [SerializeField] private List<InteractionEffect> failureEffects;
        [SerializeField] private List<InteractionEffect> cancelEffects;

        [SerializeField] private bool destroyAfterSuccess = false;
        
        private bool _isExecuting;
        private InteractionContext _currentCtx;

        public bool IsAvailable => !_isExecuting;

        public void StartInteraction(InteractionContext ctx)
        {
            if (!IsAvailable) return;
            _currentCtx = ctx;
            ctx.Target = this;
          //  ctx.State = InteractionState.Starting;
          // set when creating context
            // 1. Условия
            foreach (var condition in conditions)
            {
                if (!condition.Check(ctx))
                {
                    ctx.State = InteractionState.Failure;
                    condition.PlayDenyEffects(ctx);
                    ApplyPostEffects(ctx.State);
                    return;
                }
            }

            // 2. Эффекты перед стартом (текст "Нужно взломать дверь")

            foreach (var e in preExecuteEffects) e.Play(ctx);
            // 3. Исполнение
            _isExecuting = true;
            ctx.State = InteractionState.InProgress;
            UpdateStateInInteractor(ctx.State);
            executor.Execute(ctx, OnExecutorFinished);
            foreach (var e in duringExecuteEffects) e.Play(ctx);
        }

        public void CancelInteraction()
        {
            if (!_isExecuting) return;
            _currentCtx.State = InteractionState.Cancelled;
            UpdateStateInInteractor(InteractionState.Cancelled);
            if (executor.CanCancel)
                executor.Cancel(_currentCtx);
            _currentCtx.State = InteractionState.Idle;
            UpdateStateInInteractor(InteractionState.Idle);
        }

        private void OnExecutorFinished(InteractionState state)
        {
            ApplyPostEffects(state);
            _isExecuting = false;
            _currentCtx = null;
        }

        private void ApplyPostEffects(InteractionState state)
        {
            var list = state switch
            {
                InteractionState.Success => successEffects,
                InteractionState.Failure => failureEffects,
                InteractionState.Cancelled => cancelEffects,
                _ => null
            };
            _currentCtx.State = state;
            if (list == null) return;
            foreach (var e in list) e.Play(_currentCtx);

            if (state == InteractionState.Success)
            {
                _currentCtx.Interactor.UnregisterInteractive(this);
                if (destroyAfterSuccess) gameObject.SetActive(false);
            }
            UpdateStateInInteractor(state);
        }

        private void UpdateStateInInteractor(InteractionState state)
        {
            if (_currentCtx?.Interactor == null) return;
            var interactor = _currentCtx.Interactor;
            
            interactor?.SetInteractionState(state);
        }
        
    }
}