namespace Arcatech.SaveSystem
{
    /// <summary>Враг, расставленный на уровне. Учитывается в рейтинге.
    /// Убит до чекпоинта - при загрузке удаляется. Убит после - при откате
    /// сцена перезагружается, и он появляется на исходной позиции.
    /// Враги из спавнеров этот компонент игнорируют (EntityID.IsRuntimeSpawned).</summary>
    public class EnemyProgress : SavedProgressItemBase
    {
        public override ProgressItemKind Kind => ProgressItemKind.Enemy;

        protected override void OnApplySaveState(ProgressItemState state, ILevelProgressContext ctx)
        {
            // Destroy, а не SetActive(false): DeadEntityTrigger считает уничтоженную
            // сущность мёртвой, а выключенную - живой.
            if (state == ProgressItemState.Completed) Destroy(gameObject);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Entity.AnnounceDead.AddListener(HandleDeadChanged);
        }

        protected override void OnDisable()
        {
            Entity.AnnounceDead.RemoveListener(HandleDeadChanged);
            base.OnDisable();
        }

        private void HandleDeadChanged(BaseGameEntityComponent entity)
        {
            if (!entity.EntityAlive && ReadItemState != ProgressItemState.Completed)
                Announce(ProgressItemState.Completed);
        }
    }
}
