namespace Arcatech.Managers.Save
{
    public interface ISavesService
    {
        void Save(GameSaveData data, bool overwrite = true);
        GameSaveData Load();  
    
    }

}

