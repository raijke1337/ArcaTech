using Arcatech.Managers;
using Arcatech.Texts;
using UnityEngine;

namespace Arcatech.Interactions
{
    public class ManualInteractionTrigger : InteractionTrigger, ITargetTooltip
    {
        [Space, SerializeField] Description commandDescription;

        public override void TriggerEntered(TriggerHitInfo triggerHitInfo)
        {
            if (triggerHitInfo.TargetCollider.TryGetComponent(out IInteractor interactor))
            {
                interactor.RegisterInteractive(interactableComponent);
                if (IsPlayer(interactor))
                    GameInterfaceManager.Instance.NotifyTargetInRange(this, true);
                if (interactor.Entity.ShowingDebugs) Debug.Log($"{interactor.Entity.GetName} has entered trigger area {name}");
            }
        }

        public override void TriggerExited(TriggerHitInfo triggerExitInfo)
        {
            if (triggerExitInfo.TargetCollider.TryGetComponent(out IInteractor interactor))
            {
                interactor.UnregisterInteractive(interactableComponent);
                // TryGetInstance: при выгрузке сцены менеджер может быть уже уничтожен.
                if (IsPlayer(interactor))
                    GameInterfaceManager.TryGetInstance()?.NotifyTargetInRange(this, false);
                if (interactor.Entity.ShowingDebugs) Debug.Log($"{interactor.Entity.GetName} has exited trigger area {name}");
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            // Объект выключен или уничтожен - подсказка пропадает сразу, не дожидаясь следующего кадра.
            GameInterfaceManager.TryGetInstance()?.OnTargetableDisabled(this);
        }

        private static bool IsPlayer(IInteractor interactor) =>
            interactor.Entity != null && interactor.Entity.CompareTag("Player");

        #region ITargetTooltip

        public Description GetInfo => commandDescription;

        // Пока идёт взаимодействие или объект "выключен" - подсказку не показываем.
        public bool CanShowTooltip =>
            commandDescription != null &&
            isActiveAndEnabled &&
            interactableComponent != null &&
            interactableComponent.IsAvailable;

        public Vector3 TooltipAnchor =>
            interactableComponent != null && interactableComponent.Entity != null && interactableComponent.Entity.EffectSpawn != null
                ? interactableComponent.Entity.EffectSpawn.position
                : transform.position;

        #endregion
    }
}
