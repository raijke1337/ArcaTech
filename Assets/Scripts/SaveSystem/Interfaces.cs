using System;

namespace Arcatech.SaveSystem
{
    public interface ISaveLoadService
    {
        public bool SaveData(GameData data);
        public LoadDataResult TryLoadData(out GameData data);
    }

    public interface ISaveable
    {
        public void PopulateSaveData(GameData data);
    }

    public interface ISavedProgressItem :IComparable<ISavedProgressItem>
    {
        public string ItemID { get; }
        public bool Completed { get; set; }
        public event SimpleEventsHandler<ISavedProgressItem> UpdateEvent;
    }
}