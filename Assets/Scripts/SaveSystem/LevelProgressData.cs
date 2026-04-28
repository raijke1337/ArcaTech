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
                    Completed = new Dictionary<string, bool>(Completed)
                });
            }
            else
            {
                existing.Completed ??= new Dictionary<string, bool>();
                foreach (var kvp in Completed)
                {
                    existing.Completed[kvp.Key] = kvp.Value;
                }
            }
        }
    }
}