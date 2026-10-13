using KBCore.Refs;
using UnityEngine;

namespace Arcatech.SaveSystem
{
    /// <summary>Базовый класс для всех сохраняемых объектов прогресса на уровне
    /// (враги, собираемые предметы, активируемые триггеры и т.п.).
    /// Сам регистрируется/отписывается в LevelProgressController.
    /// Объекты, созданные во время игры (EntityID.IsRuntimeSpawned), не регистрируются.</summary>
    [RequireComponent(typeof(BaseGameEntityComponent))]
    public abstract class SavedProgressItemBase : ValidatedMonoBehaviour, ISavedProgressItem
    {
        [SerializeField, Self] private BaseGameEntityComponent entity;
        protected BaseGameEntityComponent Entity => entity;

        public string SavedItemID => entity.GetID;
        public string Name => entity.GetName;
        public abstract ProgressItemKind Kind { get; }

        /// <summary>Объект создан во время игры - его состояние не сохраняется.</summary>
        public bool IsTracked => entity != null && entity.TryGetComponent(out EntityID id) && !id.IsRuntimeSpawned;

        private ProgressItemState _state;
        public ProgressItemState ReadItemState => _state;

        /// <summary>Вызывается контроллером при загрузке уровня с чекпоинта
        /// (с задержкой до LateUpdate, а не внутри OnEnable).</summary>
        public void ApplySaveState(ProgressItemState state, ILevelProgressContext ctx)
        {
            _state = state;
            OnApplySaveState(state, ctx);
        }

        /// <summary>Привести объект на сцене к сохранённому состоянию.</summary>
        protected abstract void OnApplySaveState(ProgressItemState state, ILevelProgressContext ctx);

        protected virtual void OnEnable()
        {
            if (IsTracked) LevelProgressController.Instance.Register(this);
        }

        protected virtual void OnDisable()
        {
            LevelProgressController.TryGetInstance()?.Unregister(this);
        }

        /// <summary>Вызывать из наследника при каждом изменении состояния
        /// (подобрали предмет, убили врага и т.п.).</summary>
        protected void Announce(ProgressItemState state)
        {
            _state = state;
            if (IsTracked) LevelProgressController.Instance.SavedItemAnnounce(this);
        }
    }
}
