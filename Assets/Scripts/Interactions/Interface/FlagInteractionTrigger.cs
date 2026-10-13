using System.Collections;
using System.Collections.Generic;
using Arcatech.SaveSystem;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Interactions
{
    /// <summary>
    /// Запускает InteractableComponent, когда выставлен флаг (или флаги).
    /// Событийный: реагирует на ObjectiveFlagSO.Changed, без опроса в Update.
    /// Если флаги уже выставлены на момент включения объекта - срабатывает сразу.
    /// Использует общие поля InteractionTrigger: triggerOnce, cooldown.
    /// </summary>
    public class FlagInteractionTrigger : InteractionTrigger
    {
        [Header("Flags")]
        [SerializeField] private List<ObjectiveFlagSO> flags;
        [Tooltip("true - нужны все флаги из списка, false - достаточно любого")]
        [SerializeField] private bool requireAll = true;

        private Coroutine _fireRoutine;
        private InteractionComponent _interactor;

        // Зональные события этому триггеру не нужны
        public override void TriggerEntered(TriggerHitInfo triggerHitInfo) { }
        public override void TriggerExited(TriggerHitInfo triggerExitInfo) { }

        private void OnEnable()
        {
            if (flags != null)
                foreach (var f in flags)
                    if (f != null) f.Changed += HandleFlagChanged;

            // Флаги могли быть выставлены до включения объекта
            TryFire();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (flags != null)
                foreach (var f in flags)
                    if (f != null) f.Changed -= HandleFlagChanged;

            // Unity останавливает корутину при выключении, но поле само не обнуляется
            _fireRoutine = null;
        }

        private void HandleFlagChanged() => TryFire();

        private bool FlagsSatisfied()
        {
            if (flags == null) return false;

            int total = 0, set = 0;
            foreach (var f in flags)
            {
                if (f == null) continue;
                total++;
                if (f.IsSet) set++;
            }

            if (total == 0) return false;
            return requireAll ? set == total : set > 0;
        }

        private void TryFire()
        {
            if (!FlagsSatisfied()) return;
            if (triggerOnce && HasTriggered) return;
            if (_fireRoutine != null) return; // уже ждём возможности запуститься

            _fireRoutine = StartCoroutine(FireWhenAvailable());
        }

        private IEnumerator FireWhenAvailable()
        {
            try
            {
                if (interactableComponent == null) yield break;

                // cooldown из базового класса
                float wait = LastTriggerTime + cooldown - Time.time;
                if (wait > 0f) yield return new WaitForSeconds(wait);

                // InteractableComponent выставляет _listening в своём Start и может быть
                // занят предыдущим взаимодействием - ждём, пока можно стартовать
                while (!interactableComponent.IsAvailable) yield return null;

                // За время ожидания флаг могли сбросить
                if (!FlagsSatisfied()) yield break;

                interactableComponent.StartInteraction(new InteractionContext
                {
                    Interactor = ResolveInteractor(),
                    // Target обязателен: StartInteraction обращается к ctx.Interactor.Entity,
                    // только если Target пуст (тот же приём, что в DeadEntityTrigger)
                    Target = interactableComponent.Entity,
                    State = InteractionState.Starting
                });

                HasTriggered = true;
                LastTriggerTime = Time.time;
            }
            finally
            {
                _fireRoutine = null;
            }
        }

        private InteractionComponent ResolveInteractor()
        {
            if (_interactor != null) return _interactor;

            var player = FindAnyObjectByType<PlayerComponent>();
            if (player != null) _interactor = player.GetComponent<InteractionComponent>();
            return _interactor;
        }
    }
}