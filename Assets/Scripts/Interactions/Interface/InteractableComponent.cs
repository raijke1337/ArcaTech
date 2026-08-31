using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Arcatech.SaveSystem;
using KBCore.Refs;
using Unity.U2D.Physics;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Interactions
{
    /// <summary>
    /// item to be interacted with
    /// </summary>
    [RequireComponent(typeof(BaseGameEntityComponent))]
    public class InteractableComponent : ValidatedMonoBehaviour
    {
        [SerializeField, Self] private BaseGameEntityComponent entity;
        public BaseGameEntityComponent Entity => entity;

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

        private bool _listening = false; // activated after load level condition
        public bool IsAvailable => !_isExecuting && _listening && executor != null;

        private void OnDisable()
        {
            if (_isExecuting)
                CancelInteraction();
        }

        private void Start()
        {
            _listening = true; // PLACEHOLDER
        }

        public void StartInteraction(InteractionContext ctx)
        {
            if (!IsAvailable) return;

            if (!ctx.Target && !ctx.Interactor.Entity) return;

            // Инвалидируем старый execution и блокируем новый
            _executionId++;
            StopAllCoroutines();
            _isExecuting = true;

            StartCoroutine(RunPipeline(ctx));
        }

        private IEnumerator RunPipeline(InteractionContext ctx)
        {

            _currentCtx = ctx;
            ctx.Target = entity;

            // ─── 1. Conditions ───
            foreach (var condition in conditions)
            {
                if (condition == null) continue;
                if (!condition.Check(ctx))
                {
                    ctx.State = InteractionState.Failure;
                    condition.PlayDenyEffects(ctx);
                    yield return ExecuteBlockingEffects(null, ctx, InteractionState.Failure);
                    _isExecuting = false;
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
            StateChangedEvent?.Invoke(state);
        }

        public Action<InteractionState> StateChangedEvent;

        public void ForceState(ProgressItemState state)
        {
            // Если состояние уже завершено или отменено, ничего делать не нужно
            if (state != ProgressItemState.Completed) return;

            // Создаем фиктивный контекст для проигрывания эффектов.
            // Важно: Interactor может быть null при загрузке, если игрок далеко, 
            // но эффекты должны уметь это обрабатывать или мы передаем заглушку.
            var ctx = new InteractionContext
            {
                Target = entity,
                State = InteractionState.Starting
            };

            // 1. Pre-Execute Effects
            // Выполняем только те, что помечены для воспроизведения при загрузке
            if (preExecuteEffects != null)
            {
                foreach (var effect in preExecuteEffects)
                {
                    if (effect != null && effect.ReplayOnLoad)
                    {
                        effect.Play(ctx);
                    }
                }
            }

            // 2. Executor пропускаем полностью (избегаем мини-игр и логики выполнения)

            // 3. During Execute Effects
            if (duringExecuteEffects != null)
            {
                foreach (var effect in duringExecuteEffects)
                {
                    if (effect != null && effect.ReplayOnLoad)
                    {
                        effect.Play(ctx);
                    }
                }
            }

            // 4. Success Effects (так как ForceState обычно вызывается для Completed состояния в прогрессе)
            // Если логика игры подразумевает, что ForceState(Completed) означает успешное завершение в прошлом
            if (state == ProgressItemState.Completed) 
            {
                ctx.State = InteractionState.Success;
                
                if (successEffects != null)
                {
                    foreach (var effect in successEffects)
                    {
                        if (effect != null && effect.ReplayOnLoad)
                        {
                            effect.Play(ctx);
                        }
                    }
                }
                
                // Если объект должен был уничтожиться после успеха, делаем это сейчас
                if (destroyAfterSuccess)
                {
                    gameObject.SetActive(false);
                }
            }
            
            // Сбрасываем флаг исполнения, чтобы объект снова стал интерактивным (если это задумано дизайном)
            // Или оставляем _isExecuting = false, так как мы не запускали корутину
            _isExecuting = false;
        }
    }
}