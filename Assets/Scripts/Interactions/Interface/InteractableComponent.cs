using System.Collections;
using System.Collections.Generic;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Interactions
{
    /// <summary>
    /// item to be interacted with
    /// </summary>
    [RequireComponent(typeof(BaseGameEntityComponent))]
    public class InteractableComponent : ValidatedMonoBehaviour
    {
        [SerializeField, Self] private BaseGameEntityComponent entity;

        [Header("Pipeline")] [SerializeField, Self]
        protected InteractionTrigger trigger;

        [SerializeField] private List<InteractionCondition> conditions;
        [SerializeField] private List<InteractionEffect> preExecuteEffects;
        [SerializeField] private InteractionExecutor executor;
        [SerializeField] private List<InteractionEffect> duringExecuteEffects;

        [Header("Post Effects")] [SerializeField]
        private List<InteractionEffect> successEffects;

        [SerializeField] private List<InteractionEffect> failureEffects;
        [SerializeField] private List<InteractionEffect> cancelEffects;

        [SerializeField] private bool destroyAfterSuccess;

        private bool _isExecuting;
        private InteractionContext _currentCtx;
        private int _executionId; // защита от stale callbacks

        public bool IsAvailable => !_isExecuting && executor != null;

        private void OnDisable()
        {
            if (_isExecuting)
                CancelInteraction();
        }

        public void StartInteraction(InteractionContext ctx)
        {
            if (!IsAvailable) return;

            // Инвалидируем старый execution и блокируем новый
            _executionId++;
            StopAllCoroutines();
            _isExecuting = true;

            StartCoroutine(RunPipeline(ctx));
        }

        private IEnumerator RunPipeline(InteractionContext ctx)
        {
            _currentCtx = ctx;
            ctx.Target = this;

            // ─── 1. Conditions ───
            foreach (var condition in conditions)
            {
                if (condition == null) continue;
                if (!condition.Check(ctx))
                {
                    ctx.State = InteractionState.Failure;
                    condition.PlayDenyEffects(ctx);
                    yield return ExecuteBlockingEffects(null, ctx, InteractionState.Failure);
                    ApplyPostEffects(InteractionState.Failure);
                    yield break;
                }
            }

            // ─── 2. Pre-Execute ───
            // Теперь State = Starting гарантированно уйдёт в InteractionComponent
            yield return ExecuteBlockingEffects(preExecuteEffects, ctx, InteractionState.Starting);

            // ─── 3. Executor ───
            ctx.State = InteractionState.InProgress;
            UpdateStateInInteractor(ctx.State);

            foreach (var e in duringExecuteEffects)
                e?.Play(ctx);

            bool executorDone = false;
            InteractionState result = InteractionState.Failure;

            // Локальный id, чтобы отсечь поздние колбэки после Cancel
            int myExecutionId = _executionId;

            executor.Execute(ctx, r =>
            {
                if (myExecutionId != _executionId) return; // игнорируем устаревший колбэк
                result = r;
                executorDone = true;
            });

            yield return new WaitUntil(() => executorDone);

            OnExecutorFinished(result);
        }

        /// <summary>
        /// Запускает эффекты и ждёт блокирующие. Не аллоцирует List.
        /// </summary>
        private IEnumerator ExecuteBlockingEffects(List<InteractionEffect> effects, InteractionContext ctx,
            InteractionState stageState)
        {
            ctx.State = stageState;
            UpdateStateInInteractor(stageState);

            if (effects == null || effects.Count == 0) yield break;

            int blockerCount = 0;
            for (int i = 0; i < effects.Count; i++)
            {
                var e = effects[i];
                if (e == null) continue;
                e.Play(ctx);
                if (e.IsBlocking) blockerCount++;
            }

            if (blockerCount == 0) yield break;

            while (true)
            {
                int remaining = 0;
                for (int i = 0; i < effects.Count; i++)
                {
                    var e = effects[i];
                    if (e != null && e.IsBlocking && !e.IsBlockingComplete)
                        remaining++;
                }

                if (remaining == 0) break;
                yield return null;
            }
        }

        public void CancelInteraction()
        {
            if (!_isExecuting) return;
            _isExecuting = false;

            StopAllCoroutines();

            // Оповещаем эффекты, чтобы сняли блокировки
            if (preExecuteEffects != null)
                foreach (var e in preExecuteEffects)
                    e?.OnCancelled();
            if (duringExecuteEffects != null)
                foreach (var e in duringExecuteEffects)
                    e?.OnCancelled();

            executor?.Cancel(_currentCtx);

            if (_currentCtx != null)
            {
                _currentCtx.State = InteractionState.Cancelled;
                ApplyPostEffects(InteractionState.Cancelled);
            }

            _executionId++; // инвалидируем текущий pipeline
            _currentCtx = null;
        }

        private void OnExecutorFinished(InteractionState state)
        {
            _isExecuting = false;
            ApplyPostEffects(state);
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

            if (_currentCtx != null)
            {
                _currentCtx.State = state;
                if (list != null)
                {
                    foreach (var e in list)
                        e?.Play(_currentCtx);
                }

                if (state == InteractionState.Success)
                {
                    _currentCtx.Interactor?.UnregisterInteractive(this);
                    if (destroyAfterSuccess) gameObject.SetActive(false);
                }
            }

            UpdateStateInInteractor(state);

            // После терминального статуса явно сбрасываем интерактор в Idle (через задержку в InteractionComponent)
            if (state is InteractionState.Success or InteractionState.Failure or InteractionState.Cancelled)
            {
                _currentCtx?.Interactor?.ResetToIdle();
            }
        }

        private void UpdateStateInInteractor(InteractionState state)
        {
            _currentCtx?.Interactor?.SetInteractionState(state);
        }
    }
}