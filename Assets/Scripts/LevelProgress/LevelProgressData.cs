using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Arcatech.SaveSystem
{
    [Serializable]
    public class LevelProgressData : ISaveable
    {
        public string levelID;
        public Dictionary<string, bool> Completed = new();
        public SerializableVector3 resumePosition;
        public LevelProgressData()
        {
        }

        public LevelProgressData(LevelProgressData other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            levelID = other.levelID;
            resumePosition = other.resumePosition;
            Completed = other.Completed != null
                ? other.Completed.ToDictionary(pair => pair.Key, pair => pair.Value)
                : new Dictionary<string, bool>();
        }
        
        public void PopulateSaveData(GameData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (string.IsNullOrWhiteSpace(levelID))
            {
                Debug.LogWarning("LevelProgressData has empty levelID, skipping save update.");
                return;
            }

            var existing = data.levelRecords.FirstOrDefault(r => r.levelID == levelID);
            if (existing == null)
            {
                data.levelRecords.Add(new LevelProgressData
                {
                    levelID = levelID,
                    Completed = new Dictionary<string, bool>(Completed),
                    resumePosition = resumePosition
                });
            }
            else
            {
                existing.Completed ??= new Dictionary<string, bool>();

                // Snapshot the pairs so mutations don’t affect the enumerator.
                var updates = Completed?.ToArray() ?? Array.Empty<KeyValuePair<string, bool>>();

                foreach (var kvp in updates)
                {
                    existing.Completed[kvp.Key] = kvp.Value;
                }
                
                existing.resumePosition = resumePosition;
            }
        }
    }
}