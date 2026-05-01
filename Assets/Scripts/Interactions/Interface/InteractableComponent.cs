using System.Collections.Generic;
using Arcatech.Managers;
using Arcatech.Texts;
using Arcatech.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Arcatech.Interactions
{
    /// <summary>
    /// item to be interacted with
    /// </summary>
    public class InteractableComponent : MonoBehaviour, ITriggerNotificationReceiver, ITargetable
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

            // 1. Условия
            foreach (var condition in _conditions)
            {
                if (!condition.Check(ctx))
                {
                    condition.PlayDenyEffects(ctx);
                    ApplyPostEffects(InteractionStatus.Failure);
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

        private void OnExecutorFinished(InteractionStatus status)
        {
            ApplyPostEffects(status);
            _isExecuting = false;
            SetPlayerLock(false);
            _currentCtx = null;
        }

        private void ApplyPostEffects(InteractionStatus status)
        {
            var list = status switch
            {
                InteractionStatus.Success => _successEffects,
                InteractionStatus.Failure => _failureEffects,
                InteractionStatus.Cancelled => _cancelEffects,
                _ => null
            };
            _currentCtx.FinalStatus = status;
            if (list == null) return;
            foreach (var e in list) e.Play(_currentCtx);
        }

        private void SetPlayerLock(bool locked)
        {
            if (_currentCtx?.Interactor == null) return;
            var interactor = _currentCtx.Interactor;
            
            interactor?.SetInteractionLock(locked);
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