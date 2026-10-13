using System;
using System.Collections.Generic;
using System.Linq;

namespace Arcatech.SaveSystem
{
    /// <summary>
    /// Прогресс игрока в прохождении игры. Обновляется при завершении уровня
    /// и при выходе после смерти (инкремент финальной сцены). Всё, что игрок
    /// нашёл на уровне, попадает сюда ТОЛЬКО при прохождении уровня -
    /// до этого оно живёт в чекпоинте (LevelProgressData) и в Мастерской не видно.
    /// </summary>
    [Serializable]
    public class MetaProgressData : VersionedSaveData
    {
        /// <summary>Детали - валюта для разблокировки чертежей в Мастерской.</summary>
        public int partsCount;

        /// <summary>Вся экипировка (ID EquipSO), которая есть у игрока.</summary>
        public HashSet<string> unlockedWeapons = new();

        /// <summary>Экипировка, назначенная в интерфейсе «Персонаж» перед уровнем.
        /// С ней Теилс выходит на уровень. Пусто - берётся пресет игрока по умолчанию.</summary>
        public List<string> equippedLoadout = new();

        // Чертёж: найден на уровне -> разблокирован в Мастерской за детали.
        public HashSet<string> foundCostumeBlueprints = new();
        public HashSet<string> unlockedCostumeBlueprints = new();
        public HashSet<string> foundGalleryBlueprints = new();
        public HashSet<string> unlockedGalleryBlueprints = new();

        /// <summary>0..1 - прогресс сцены окончания игры.</summary>
        public float endingSceneProgress;

        public List<LevelMetaRecord> levelRecords = new();

        public bool HasAnyProgress =>
            levelRecords.Any(r => r.isCompleted) || partsCount > 0 || unlockedWeapons.Count > 0;

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

        public bool TryGetLevelRecord(string levelID, out LevelMetaRecord record)
        {
            record = levelRecords.FirstOrDefault(r => r.levelID == levelID);
            return record != null;
        }

        /// <summary>Разблокировка найденного чертежа в Мастерской. Списывает детали.
        /// После успешного вызова нужно SaveManager.SaveMetaProgress().</summary>
        public bool TryUnlockBlueprint(string blueprintID, int partsCost)
        {
            if (partsCount < partsCost) return false;

            if (foundCostumeBlueprints.Remove(blueprintID))
            {
                unlockedCostumeBlueprints.Add(blueprintID);
            }
            else if (foundGalleryBlueprints.Remove(blueprintID))
            {
                unlockedGalleryBlueprints.Add(blueprintID);
            }
            else
            {
                return false; // не найден или уже разблокирован
            }

            partsCount -= partsCost;
            return true;
        }
    }
}
