using System;

namespace Arcatech.SaveSystem
{
    /// <summary>Мета-данные одного уровня: открыт ли он и лучший результат.
    /// Используется интерфейсами «Информация об уровне» и «Сводка прохождения».</summary>
    [Serializable]
    public class LevelMetaRecord
    {
        public string levelID;
        public bool isUnlocked;
        public bool isCompleted;

        public LevelGrade bestGrade = LevelGrade.None;
        public float bestScore;
        public float bestKillsShare;
        public float bestItemsShare;

        public void RegisterCompletion(LevelResult result)
        {
            isUnlocked = true;
            isCompleted = true;

            if (result.score > bestScore)
            {
                bestScore = result.score;
                bestGrade = result.grade;
            }

            // Лучшие доли считаются независимо: «лучший забег по врагам» и
            // «лучший забег по предметам» могут быть разными прохождениями.
            if (result.killsShare > bestKillsShare) bestKillsShare = result.killsShare;
            if (result.itemsShare > bestItemsShare) bestItemsShare = result.itemsShare;
        }
    }
}
