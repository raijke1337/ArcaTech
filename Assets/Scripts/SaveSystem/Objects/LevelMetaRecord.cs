using System;

namespace Arcatech.SaveSystem
{
    [Serializable]
    public class LevelMetaRecord
    {
        public string levelID;
        public bool isUnlocked;
        public int bestRating;

        public void RegisterCompletion(int rating)
        {
            isUnlocked = true;
            if (rating > bestRating) bestRating = rating;
        }
    }
}