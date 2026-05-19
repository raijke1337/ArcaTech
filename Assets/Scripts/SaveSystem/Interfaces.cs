using System;
using UnityEngine;

namespace Arcatech.SaveSystem
{
    public interface ISaveLoadService
    {
        public bool SaveData(GameData data);
        public LoadDataResult TryLoadData(out GameData data);
    }

    public abstract class SaveService : ScriptableObject, ISaveLoadService
    {
        public abstract bool SaveData(GameData data);
        public abstract LoadDataResult TryLoadData(out GameData data);
    }

    public interface ISaveable
    {
        public void PopulateSaveData(GameData data);
        public void NotifyForUpdate();
    }

    public interface ISavedProgressItem
    {
        public string SavedItemID { get; }
        public ProgressItemState ReadItemState { get; }
        public void ApplySaveState(ProgressItemState state, LevelProgressManager ctx);
        public string Name { get; }
    }
}