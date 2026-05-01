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
    public class InteractableComponent : ValidatedMonoBehaviour, ITriggerNotificationReceiver, ITargetable
    {
        [Header("Anchor")] [SerializeField] private Transform _interactionPoint;
        [Header("Pipeline")] [SerializeField] private List<InteractionCondition> _conditions;
        [SerializeField] private List<InteractionEffect> _preExecuteEffects;
        [SerializeField] private InteractionExecutor _executor;

        [Header("Post Effects")] [SerializeField]
        private List<InteractionEffect> _successEffects;

        [SerializeField] private List<InteractionEffect> _failureEffects;
        [SerializeField] private List<InteractionEffect> _cancelEffects;
        [SerializeField] protected TriggerTrackerComponent activationArea;
        
        [Space,SerializeField] Description description;
        [SerializeField] private bool destroyAfterSuccess = false;
        [SerializeField, Self] private BaseGameEntityComponent entity;
        
        
        public Description GetInfo =>  description;
        
        private bool _isExecuting;
        private InteractionContext _currentCtx;

        public Transform InteractionPoint => _interactionPoint;
        public bool IsAvailable => !_isExecuting;

        public void StartInteraction(InteractionContext ctx)
        {
            if (!IsAvailable) return;
            _currentCtx = ctx;
            ctx.Target = this;
            ctx.State = InteractionState.Success;
            // 1. Условия
            foreach (var condition in _conditions)
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
            foreach (var e in _preExecuteEffects) e.Play(ctx);

            // 3. Исполнение
            _isExecuting = true;
            SetPlayerLock(true);

            _executor.Execute(ctx, OnExecutorFinished);
        }

        public void CancelInteraction()
        {
            if (!_isExecuting) return;
            if (_executor.CanCancel)
                _executor.Cancel(_currentCtx);
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
                InteractionState.Success => _successEffects,
                InteractionState.Failure => _failureEffects,
                InteractionState.Cancelled => _cancelEffects,
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

        public void TriggerEntered(TriggerHitInfo triggerHitInfo)
        {
            if (triggerHitInfo.TargetCollider.TryGetComponent(out IInteractor interactor))
            {
                interactor.RegisterInteractive(this);
            }
        }

        public void TriggerExited(TriggerHitInfo triggerExitInfo)
        {
            if (triggerExitInfo.TargetCollider.TryGetComponent(out IInteractor interactor))
            {
                interactor.UnregisterInteractive(this);
            }
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            GameInterfaceManager.Instance?.NotifyTargetable(this,true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            GameInterfaceManager.Instance?.NotifyTargetable(this,false);
        }
        protected virtual void Start()
        {
            activationArea.Active = true;
            activationArea.RegisterReceiver(this);
        }
        protected virtual void OnDisable()
        {
            activationArea.UnregisterReceiver(this);
        }
    }
}