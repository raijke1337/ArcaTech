using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Arcatech.SaveSystem
{
    public enum ProgressItemState
    {
        Default,
        Completed,
        Failed
    }
    
    [Serializable]
    public class LevelProgressData : ISaveable
    {
        public string levelID;
        public Dictionary<string, ProgressItemState> ProgressItemStates = new();
        public SerializableVector3 resumePosition;
        public LevelProgressData()
        {}

        public LevelProgressData(LevelProgressData other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            levelID = other.levelID;
            resumePosition = other.resumePosition;
            ProgressItemStates = other.ProgressItemStates != null
                ? other.ProgressItemStates.ToDictionary(pair => pair.Key, pair => pair.Value)
                : new Dictionary<string, ProgressItemState>();
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
                    ProgressItemStates = new Dictionary<string, ProgressItemState>(ProgressItemStates),
                    resumePosition = resumePosition
                });
            }
            else
            {
                existing.ProgressItemStates ??= new Dictionary<string, ProgressItemState>();

                // Snapshot the pairs so mutations don’t affect the enumerator.
                var updates = ProgressItemStates?.ToArray() ?? Array.Empty<KeyValuePair<string, ProgressItemState>>();

                foreach (var kvp in updates)
                {
                    existing.ProgressItemStates[kvp.Key] = kvp.Value;
                }
                
                existing.resumePosition = resumePosition;
            }
        }

        public void NotifyForUpdate()
        { }
    }
}