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
    }

    public interface ISavedProgressItem :IComparable<ISavedProgressItem>
    {
        public string SavedItemID { get; }
        public bool SavedItemState { get; set; }
        public event SimpleEventsHandler<ISavedProgressItem> UpdateEvent;
    }
}