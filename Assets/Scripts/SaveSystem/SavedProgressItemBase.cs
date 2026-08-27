using Arcatech.Managers;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.SaveSystem
{
    /// <summary>Базовый класс для всех сохраняемых объектов прогресса на уровне
    /// (враги, собираемые предметы, активируемые триггеры и т.п.).
    /// Сам регистрируется/отписывается в LevelProgressController - без сканирования
    /// сцены и без риска рассинхронизации ID с BaseGameEntityComponent.</summary>
    [RequireComponent(typeof(BaseGameEntityComponent))]
    public abstract class SavedProgressItemBase : ValidatedMonoBehaviour, ISavedProgressItem
    {
        [SerializeField, Self] private BaseGameEntityComponent entity;

        public string SavedItemID => entity.GetID;
        public string Name => entity.GetName;

        public abstract ProgressItemState ReadItemState { get; }
        public abstract void ApplySaveState(ProgressItemState state, ILevelProgressContext ctx);

        protected virtual void OnEnable()
        {
            LevelProgressController.Instance.Register(this);
        }

        protected virtual void OnDisable()
        {
            LevelProgressController.TryGetInstance()?.Unregister(this);
        }

        /// <summary>Вызывать из наследника при каждом изменении состояния
        /// (подобрали предмет, убили врага и т.п.).</summary>
        protected void Announce()
        {
            LevelProgressController.Instance.SavedItemAnnounce(this);
        }
    }
}