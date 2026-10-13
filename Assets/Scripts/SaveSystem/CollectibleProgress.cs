using Arcatech.Interactions;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.SaveSystem
{
    /// <summary>Собираемый предмет на уровне (с ItemPickUpEffect). Учитывается в рейтинге.
    /// Подобранный до чекпоинта предмет при загрузке просто скрывается - сам предмет
    /// уже лежит в сохранённом инвентаре.</summary>
    [RequireComponent(typeof(InteractableComponent))]
    public class CollectibleProgress : SavedProgressItemBase
    {
        [SerializeField, Self] private InteractableComponent comp;

        public override ProgressItemKind Kind => ProgressItemKind.Collectible;

        protected override void OnApplySaveState(ProgressItemState state, ILevelProgressContext ctx)
        {
            if (state == ProgressItemState.Completed) gameObject.SetActive(false);
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
