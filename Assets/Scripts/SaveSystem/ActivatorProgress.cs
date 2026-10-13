using Arcatech.Interactions;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.SaveSystem
{
    /// <summary>Сохраняет состояние интерактивного объекта (дверь, консоль, рычаг).
    /// При загрузке с чекпоинта завершённый объект проигрывает эффекты с ReplayOnLoad.</summary>
    [RequireComponent(typeof(InteractableComponent))]
    public class ActivatorProgress : SavedProgressItemBase
    {
        [SerializeField, Self] private InteractableComponent comp;

        public override ProgressItemKind Kind => ProgressItemKind.Activator;

        protected override void OnApplySaveState(ProgressItemState state, ILevelProgressContext ctx)
        {
            comp.ForceState(state);
        }

        // Подписка на всё время жизни объекта, а не на время, пока он включён:
        // объект может выключиться в момент успешного взаимодействия (destroyAfterSuccess),
        // и событие об успехе не должно потеряться.
        private void Awake()
        {
            comp.StateChangedEvent += HandleEvent;
        }

        private void OnDestroy()
        {
            if (comp != null) comp.StateChangedEvent -= HandleEvent;
        }

        private void HandleEvent(InteractionState state)
        {
            if (state == InteractionState.Success) Announce(ProgressItemState.Completed);
        }
    }
}
