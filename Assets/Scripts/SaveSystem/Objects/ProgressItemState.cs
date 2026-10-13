namespace Arcatech.SaveSystem
{
    public enum ProgressItemState
    {
        Default,
        Completed,
        Failed
    }

    /// <summary>Тип объекта прогресса. Нужен для подсчёта рейтинга
    /// (доля убитых врагов, доля собранных предметов).</summary>
    public enum ProgressItemKind
    {
        Activator,
        Checkpoint,
        Enemy,
        Collectible,
        Secret // плейсхолдер: триггер секретной зоны, пока не используется
    }
}
