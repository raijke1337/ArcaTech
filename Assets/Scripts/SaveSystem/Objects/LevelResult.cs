using System;

namespace Arcatech.SaveSystem
{
    /// <summary>Буквенный рейтинг прохождения уровня. None - уровень не пройден.</summary>
    public enum LevelGrade
    {
        None,
        C,
        B,
        A,
        S
    }

    /// <summary>Итог прохождения уровня. Доли - 0..1.</summary>
    [Serializable]
    public struct LevelResult
    {
        public LevelGrade grade;
        public float score;
        public float livesShare;
        public float killsShare;
        public float itemsShare;

        // Абсолютные значения для окна «Сводка прохождения уровня»
        public int livesRemaining;
        public int maxLives;
        public int enemiesKilled;
        public int enemiesTotal;
        public int itemsCollected;
        public int itemsTotal;
        public int secretsFound;   // секреты пока плейсхолдер: в рейтинге не участвуют
        public int secretsTotal;

        /// <summary>Результат лучше прежнего лучшего (или уровень пройден впервые).</summary>
        public bool isNewBest;

        public override string ToString() =>
            $"{grade} (score {score:P0}: lives {livesShare:P0}, kills {killsShare:P0}, items {itemsShare:P0})";
    }
}
