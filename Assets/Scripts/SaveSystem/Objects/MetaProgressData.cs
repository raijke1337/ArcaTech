using System;
using System.Collections.Generic;
using System.Linq;

namespace Arcatech.SaveSystem
{


    /// <summary>
    /// Прогресс игрока в прохождении игры. Обновляется при завершении уровня,
    /// при выходе после смерти (небольшой инкремент) и НИКОГДА при abandon.
    /// Не хранит ничего специфичного для конкретной игровой сессии на уровне
    /// (чекпоинты, текущий инвентарь и т.п. живут только в LevelProgressData).
    /// </summary>
    [Serializable]
    public class MetaProgressData : VersionedSaveData
    {
        public int partsCount;
        public HashSet<string> unlockedWeapons = new();
        public HashSet<string> unlockedCostumeBlueprints = new();
        public HashSet<string> unlockedGalleryBlueprints = new();

        /// <summary>0..1 - прогресс сцены окончания игры.</summary>
        public float endingSceneProgress;

        public List<LevelMetaRecord> levelRecords = new();

        public LevelMetaRecord GetOrCreateLevelRecord(string levelID)
        {
            var record = levelRecords.FirstOrDefault(r => r.levelID == levelID);
            if (record == null)
            {
                record = new LevelMetaRecord { levelID = levelID };
                levelRecords.Add(record);
            }

            return record;
        }
    }
}