using System;
using System.Collections.Generic;
using System.Linq;

namespace Arcatech.SaveSystem
{
    /// <summary>
    /// Чисто runtime-объект геймплейного прогресса на уровне.
    /// НИКОГДА не сериализуется на диск и не проходит через SaveManager -
    /// при "грязном" выходе из приложения этот прогресс осознанно теряется.
    /// Живёт внутри LevelProgressController в двух экземплярах:
    /// "текущий" и "зафиксированный на чекпоинте".
    /// </summary>
    [Serializable]
    public class LevelProgressData
    {
        public string levelID;
        public Dictionary<string, ProgressItemState> progressItemStates = new();
        public SerializableVector3 resumePosition;
        public int livesRemaining;
        public SavedEntityInventory currentInventory;

        public LevelProgressData() { }

        /// <summary>Честный deep copy — включая инвентарь, чтобы checkpoint- и
        /// current-снапшоты не делили общие списки/массивы.</summary>
        public LevelProgressData(LevelProgressData other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            levelID = other.levelID;
            resumePosition = other.resumePosition;
            livesRemaining = other.livesRemaining;

            progressItemStates = other.progressItemStates != null
                ? other.progressItemStates.ToDictionary(p => p.Key, p => p.Value)
                : new Dictionary<string, ProgressItemState>();

            currentInventory = other.currentInventory != null
                ? new SavedEntityInventory(other.currentInventory)
                : null;
        }
    }
}