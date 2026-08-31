using UnityEngine;

namespace Arcatech.SaveSystem
{
    public enum LoadDataResult
    {
        Success,
        Missing,
        HashFail
    }

    /// <summary>Узкий контекст, передаваемый объектам прогресса вместо конкретного
    /// класса контроллера - убирает жёсткую зависимость ISavedProgressItem от
    /// LevelProgressController.</summary>
    public interface ILevelProgressContext
    {
        string CurrentLevelId { get; }
    }

    public interface ISaveLoadService
    {
        bool SaveData<T>(T data, string fileName) where T : class;
        LoadDataResult TryLoadData<T>(string fileName, out T data) where T : class;
    }

    public abstract class SaveService : ScriptableObject, ISaveLoadService
    {
        public abstract bool SaveData<T>(T data, string fileName) where T : class;
        public abstract LoadDataResult TryLoadData<T>(string fileName, out T data) where T : class;
    }

    /// <summary>Интерфейс объекта прогресса на сцене. Идентифицируется через
    /// BaseGameEntityComponent.GetID (см. SavedProgressItemBase).</summary>
    public interface ISavedProgressItem
    {
        string SavedItemID { get; }
        string Name { get; }
        ProgressItemState ReadItemState { get; }
        void ApplySaveState(ProgressItemState state, ILevelProgressContext ctx);
    }
}