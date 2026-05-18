
using System.Collections.Generic;
using System.Linq;
using Arcatech.Managers;
using KBCore.Refs;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Arcatech.SaveSystem
{
    public class LevelProgressManager : GenericLazySingleton<LevelProgressManager>
    {

        [SerializeField] private bool showDebugs = false;
        private string _currentLevelID;
        
        private LevelProgressData _currentProgress;
        private LevelProgressData _checkpointProgress;
        
        
        private List<ISavedProgressItem> _trackedItems;
        private BaseGameEntityComponent _player;
        
        protected void OnEnable()
        {
            _currentLevelID = SceneManager.GetActiveScene().name;
            ReadLevel();
            
            var save = SaveManager.Instance.GetGameData;
            var record = save.levelRecords.FirstOrDefault(t => t.levelID == _currentLevelID);
            if (record!= null)
            {
                // has a record, load it
                _currentProgress = new LevelProgressData(record);
                _checkpointProgress = new LevelProgressData(record);
            }
            else
            {
                // no record, create new
                _currentProgress = new LevelProgressData
                {
                    levelID = _currentLevelID,
                    ProgressItemStates = new Dictionary<string, ProgressItemState>(),
                    resumePosition = _player.transform.position.ToSerializable()
                };
                
                foreach (var item in _trackedItems)
                {
                    _currentProgress.ProgressItemStates[item.SavedItemID] =  item.ReadItemState;
                }
                _checkpointProgress = new LevelProgressData(_currentProgress);
                SaveManager.Instance.UpdateData(_currentProgress);
            }

            WriteLevel();
        }

        private void OnDisable()
        {
            _trackedItems.Clear();
            _player.AnnounceDead.RemoveListener(OnPlayerAnnounceDead);
        }

        private void ReadLevel()
        {
            _player =  FindObjectsByType<BaseGameEntityComponent>(FindObjectsSortMode.None).First(t=>t.CompareTag("Player"));
            if (_player != null)
            {
                _player.AnnounceDead.AddListener(OnPlayerAnnounceDead);
            }
            else
            {
                if (showDebugs) Debug.Log("No player found");
            }
            _trackedItems = new List<ISavedProgressItem>(FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISavedProgressItem>().ToArray());
            foreach (var item in _trackedItems)
            {
                Debug.Log($"Item {item.SavedItemID}, {item.Name}");
            }
        }

        private void WriteLevel()
        {
            foreach (var item in _trackedItems)
            {
                if (_checkpointProgress.ProgressItemStates.TryGetValue(item.SavedItemID, out var completed))
                {
                    item.ApplySaveState(completed, this);
                }
                else
                {
                    if (showDebugs)   Debug.LogWarning($"No record in save data for {item.SavedItemID}");
                }
            }
            _player.transform.position = _checkpointProgress.resumePosition.ToVector3();
            if (showDebugs) Debug.Log("Write level state completed");
        }

        public void OnCheckPointReached(CheckpointTrigger trigger)
        {
            Debug.Log($"Checkpoint found! {trigger.name}");
            _checkpointProgress = new LevelProgressData(_currentProgress)
            {
                resumePosition = trigger.transform.position.ToSerializable()
            };
        }
        public void SavedItemAnnounce(ISavedProgressItem item) => RecordItem(item);
        private void RecordItem(ISavedProgressItem item)
        {
            if (showDebugs)  Debug.Log($"Recording item state {item.ReadItemState} for {item.SavedItemID}");
            _currentProgress.ProgressItemStates[item.SavedItemID] = item.ReadItemState;
        }
        
        private void OnPlayerAnnounceDead(BaseGameEntityComponent arg0)
        {
            if (arg0.EntityAlive) return;
            _currentProgress = new LevelProgressData(_checkpointProgress);
            SaveManager.Instance.UpdateData(_checkpointProgress);
            GameInterfaceManager.Instance.ShowPlayerDeadMenu();
        }
        
        public void OnSaveData()
        {
            SaveManager.Instance.UpdateData(_checkpointProgress);
            EditorApplication.ExitPlaymode();
        }
    }
}