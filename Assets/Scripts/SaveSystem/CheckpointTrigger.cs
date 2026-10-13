using Arcatech.Triggers;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.SaveSystem
{
    /// <summary>Чекпоинт. Срабатывает один раз: фиксирует прогресс и пишет его на диск.
    /// Игрок возрождается в позиции этого объекта.</summary>
    [RequireComponent(typeof(TriggerTrackerComponent))]
    public class CheckpointTrigger : SavedProgressItemBase, ITriggerNotificationReceiver
    {
        [SerializeField, Self] private TriggerTrackerComponent trigger;

        public override ProgressItemKind Kind => ProgressItemKind.Checkpoint;

        protected override void OnEnable()
        {
            base.OnEnable();
            trigger.RegisterReceiver(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            trigger.UnregisterReceiver(this);
        }

        protected override void OnApplySaveState(ProgressItemState state, ILevelProgressContext ctx)
        {
            if (state == ProgressItemState.Completed) Deactivate();
        }

        public void TriggerEntered(TriggerHitInfo triggerHitInfo)
        {
            if (ReadItemState == ProgressItemState.Completed) return;

            if (triggerHitInfo.TryGetEntityTarget(out var tgt)
                && tgt.CompareTag("Player"))
            {
                Announce(ProgressItemState.Completed);
                LevelProgressController.Instance.OnCheckPointReached(this);
                // Не выключаем объект прямо в колбэке триггера (идёт обход списка получателей) -
                // повторные входы отсекает проверка состояния выше.
            }
        }

        public void TriggerExited(TriggerHitInfo triggerExitInfo)
        {
            //noop
        }

        // CheckpointTrigger: публичная программная активация
        public void Reach()
        {
            Announce(ProgressItemState.Completed);
            LevelProgressController.Instance.OnCheckPointReached(this);
        }
        
        private void Deactivate()
        {
            trigger.UnregisterReceiver(this);
            gameObject.SetActive(false);
        }
    }
}
