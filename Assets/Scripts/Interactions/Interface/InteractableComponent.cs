using System.Collections.Generic;
using Arcatech.Managers;
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
            ctx.State = InteractionState.Success;
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
            SetPlayerLock(true);

            executor.Execute(ctx, OnExecutorFinished);
        }

        public void CancelInteraction()
        {
            if (!_isExecuting) return;
            if (executor.CanCancel)
                executor.Cancel(_currentCtx);
        }

        private void OnExecutorFinished(InteractionState state)
        {
            ApplyPostEffects(state);
            _isExecuting = false;
            SetPlayerLock(false);
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

            if (state == InteractionState.Success && destroyAfterSuccess)
            {
                _currentCtx.Interactor.UnregisterInteractive(this);
                gameObject.SetActive(false);
            }
        }

        private void SetPlayerLock(bool locked)
        {
            if (_currentCtx?.Interactor == null) return;
            var interactor = _currentCtx.Interactor;
            
            interactor?.SetInteractionState(locked ? InteractionState.InProgress : InteractionState.Idle);
        }
    }
}