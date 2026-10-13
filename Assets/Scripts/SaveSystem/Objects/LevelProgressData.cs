using System;
using System.Collections.Generic;
using System.Linq;

namespace Arcatech.SaveSystem
{
    /// <summary>
    /// Геймплейный прогресс на уровне. Живёт в LevelProgressController в двух
    /// экземплярах: «текущий» и «зафиксированный на чекпоинте». Зафиксированный
    /// пишется на диск (SaveManager, файл чекпоинта) - по нему работают
    /// «Попробовать ещё раз» и «Продолжить» из главного меню.
    /// </summary>
    [Serializable]
    public class LevelProgressData : VersionedSaveData
    {
        public string levelID;
        public int sceneBuildIndex = -1;

        public Dictionary<string, ProgressItemState> progressItemStates = new();
        public SerializableVector3 resumePosition;
        public int livesRemaining;
        public SavedEntityInventory currentInventory;

        /// <summary>Вся экипировка, подобранная на уровне. Нужна отдельно от инвентаря:
        /// при подборе оружие заменяет надетое в том же слоте, и заменённое
        /// иначе потерялось бы для мета-прогресса.</summary>
        public HashSet<string> foundEquipmentIDs = new();

        /// <summary>Имена установленных ObjectiveFlagSO на момент чекпоинта.</summary>
        public List<string> setFlags = new();

        public LevelProgressData() { }

        /// <summary>Честный deep copy - чтобы checkpoint- и current-снапшоты
        /// не делили общие списки/словари.</summary>
        public LevelProgressData(LevelProgressData other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            version = other.version;
            timestamp = other.timestamp;
            levelID = other.levelID;
            sceneBuildIndex = other.sceneBuildIndex;
            livesRemaining = other.livesRemaining;

            resumePosition = other.resumePosition != null
                ? new SerializableVector3 { x = other.resumePosition.x, y = other.resumePosition.y, z = other.resumePosition.z }
                : null;

            progressItemStates = other.progressItemStates != null
                ? other.progressItemStates.ToDictionary(p => p.Key, p => p.Value)
                : new Dictionary<string, ProgressItemState>();

            currentInventory = other.currentInventory != null
                ? new SavedEntityInventory(other.currentInventory)
                : null;

            foundEquipmentIDs = other.foundEquipmentIDs != null
                ? new HashSet<string>(other.foundEquipmentIDs)
                : new HashSet<string>();

            setFlags = other.setFlags != null
                ? new List<string>(other.setFlags)
                : new List<string>();
        }
    }
}
