using System;
using System.Collections.Generic;
using System.Linq;
using Arcatech.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Arcatech.SaveSystem
{
    public class LevelProgressManager : GenericLazySingleton<LevelProgressManager>
    {
        private string _currentLevelID;
        private LevelProgressData _data;
        private List<ISavedProgressItem> _trackedItems;
        protected void OnEnable()
        {
            _currentLevelID = SceneManager.GetActiveScene().name;
            ReadLevel();
            var save = SaveManager.Instance.GetGameData;
            var record = save.levelRecords.FirstOrDefault(t => t.levelID == _currentLevelID);
            if (record!= null)
            {
                // has a record, load it
                _data = record;
            }
            else
            {
                // no record, create new
                _data = new LevelProgressData
                {
                    levelID = _currentLevelID,
                    Completed = new Dictionary<string, bool>()
                };
                
                foreach (var item in _trackedItems)
                {
                    _data.Completed[item.ItemID] =  item.Completed;
                }
                SaveManager.Instance.UpdateData(_data);
            }
            
            WriteLevel();
            Track(true);
        }

        private void OnDisable()
        {
            Track(false);
            _trackedItems.Clear();
        }

        private void ReadLevel()
        {
            _trackedItems = new List<ISavedProgressItem>(FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISavedProgressItem>().ToArray());
            _trackedItems.Sort();
        }

        private void WriteLevel()
        {
            foreach (var item in _trackedItems)
            {
                if (_data.Completed.TryGetValue(item.ItemID, out var completed))
                {
                    item.Completed = completed;
                }
                else
                {
                    Debug.LogWarning($"No record in save data for {item.ItemID}");
                }
            }
        }

        private void Track(bool enabling)
        {
            if (enabling)
            {
                foreach (var item in _trackedItems)
                {
                    item.UpdateEvent += WriteData;
                }
            }
            else
            {
                foreach (var item in _trackedItems)
                {
                    item.UpdateEvent -= WriteData;
                }
            }
        }

        private void WriteData(ISavedProgressItem item)
        {
            Debug.Log($"Writing data into cached record for {item.ItemID}");
            _data.Completed[item.ItemID] = item.Completed;
        }
        /// <summary>
        /// TODO: Call somewhere, externally
        /// </summary>
        private void OnLevelCompleted()
        {
            Track(false);
            SaveManager.Instance.UpdateData(_data);
        }
    }
}