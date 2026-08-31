using System;
using System.Collections.Generic;
using System.Linq;
using Arcatech.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Arcatech.SaveSystem
{
    /// <summary>
    /// Не персистентен: создаётся заново на каждой игровой сцене (уровне).
    /// Владеет геймплейным прогрессом уровня и решает, что происходит
    /// с этим прогрессом при 4 возможных исходах:
    ///   - CompleteLevel   - уровень пройден
    ///   - HandlePlayerDeath - смерть персонажа (локальный откат, без записи на диск)
    ///   - ExitAfterDeath  - игрок вышел из уровня после смерти (маленький инкремент)
    ///   - AbandonLevel    - выход через меню паузы (полный сброс)
    /// </summary>
    public class LevelProgressController : GenericLazySingleton<LevelProgressController>, ILevelProgressContext
    {
        [SerializeField] private bool showDebugs = false;
        [SerializeField] private float smallDeathExitIncrement = 0.02f;

        private string _currentLevelID;
        public string CurrentLevelId => _currentLevelID;

        private LevelProgressData _currentProgress;
        private LevelProgressData _checkpointProgress;

        private readonly Dictionary<string, ISavedProgressItem> _trackedItems = new();
        private BaseGameEntityComponent _player;

        /// <summary>Срабатывает при фиксации нового чекпоинта. Подписка опциональна
        /// (например, для UI-фидбека), в отличие от старого ISaveable-подхода.</summary>
        public event Action OnCheckpointReached;
        private bool initialized = false;
        protected void OnEnable()
        {
            if (initialized) return;
            _currentLevelID = SceneManager.GetActiveScene().name;

            _player = FindObjectsByType<BaseGameEntityComponent>(FindObjectsSortMode.None)
                .FirstOrDefault(t => t.CompareTag("Player"));

            if (_player != null)
            {
                _player.AnnounceDead.AddListener(OnPlayerAnnounceDead);
            }
            else if (showDebugs)
            {
                Debug.LogWarning("LevelProgressController: no Player found on scene.");
            }

            _currentProgress = new LevelProgressData
            {
                levelID = _currentLevelID,
                resumePosition = (_player != null ? _player.transform.position : Vector3.zero).ToSerializable()
            };
            SeedInventoryFromMeta();
            _checkpointProgress = new LevelProgressData(_currentProgress);
            
        }

        private void OnDisable()
        {
            if (_player != null)
            {
                _player.AnnounceDead.RemoveListener(OnPlayerAnnounceDead);
            }
            _trackedItems.Clear();
        }

        // ---------------------------------------------------------------
        // Регистрация объектов прогресса (заменяет FindObjectsByType-сканирование)
        // ---------------------------------------------------------------

        public void Register(ISavedProgressItem item)
        {
            if (!initialized)
            {
                OnEnable();
            }
            if (item == null || string.IsNullOrEmpty(item.SavedItemID)) return;

            _trackedItems[item.SavedItemID] = item;

            // Если объект появился на сцене позже (например, динамический спавн
            // после отката к чекпоинту) - сразу приводим его к нужному состоянию.
            
            if (_checkpointProgress.progressItemStates.TryGetValue(item.SavedItemID, out var state))
            {
                item.ApplySaveState(state, this);
            }
        }

        public void Unregister(ISavedProgressItem item)
        {
            if (item == null) return;
            if (_trackedItems.TryGetValue(item.SavedItemID, out var existing) && ReferenceEquals(existing, item))
            {
                _trackedItems.Remove(item.SavedItemID);
            }
        }

        // ---------------------------------------------------------------
        // Обновление текущего прогресса
        // ---------------------------------------------------------------

        public void SavedItemAnnounce(ISavedProgressItem item)
        {
            if (showDebugs) Debug.Log($"Recording state {item.ReadItemState} for {item.SavedItemID}");
            _currentProgress.progressItemStates[item.SavedItemID] = item.ReadItemState;
        }

        public void RecordInventory(SavedEntityInventory inventory)
        {
            _currentProgress.currentInventory = inventory;
        }

        public void RecordLives(int livesRemaining)
        {
            _currentProgress.livesRemaining = livesRemaining;
        }

        public void OnCheckPointReached(CheckpointTrigger trigger)
        {
            if (showDebugs) Debug.Log($"Checkpoint reached: {trigger.name}");

            _checkpointProgress = new LevelProgressData(_currentProgress)
            {
                resumePosition = trigger.transform.position.ToSerializable()
            };

            OnCheckpointReached?.Invoke();
        }

        // ---------------------------------------------------------------
        // Смерть персонажа - ЛОКАЛЬНЫЙ откат, SaveManager не вызывается
        // ---------------------------------------------------------------

        private void OnPlayerAnnounceDead(BaseGameEntityComponent entity)
        {
            if (entity.EntityAlive) return; // событие также стреляет при воскрешении - игнорируем
            GameInterfaceManager.Instance.ShowPlayerDeadMenu();
        }
        /// <summary>Игрок выбрал "Вернуться на чекпоинт". Восстановление in-place,
        /// БЕЗ перезагрузки сцены — иначе будет потерян _checkpointProgress
        /// (контроллер не персистентен, прогресс уровня не пишется на диск).</summary>
        public void ReturnToCheckpoint()
        {
            _currentProgress = new LevelProgressData(_checkpointProgress);
            ApplyCheckpointStateToTrackedItems();

            if (_player != null)
            {
                _player.ReviveEntity();
                // ре-использует AnnounceDead(alive) для оживления
            }
        }
        /// <summary>Игрок выбрал "Начать уровень сначала". Здесь перезагрузка сцены —
        /// правильный и самый простой способ: нужен полный чистый рестарт всего
        /// runtime-состояния уровня, а OnEnable/Awake каждого объекта для этого и
        /// предназначены. Прогресс не сохраняется никуда, meta не трогаем.</summary>
        public void RestartLevelFromScratch()
        {
            SceneManager.LoadScene(_currentLevelID);
        }

        private void ApplyCheckpointStateToTrackedItems()
        {
            foreach (var kvp in _trackedItems)
            {
                if (_checkpointProgress.progressItemStates.TryGetValue(kvp.Key, out var state))
                {
                    kvp.Value.ApplySaveState(state, this);
                }
                else
                {
                    kvp.Value.ApplySaveState(ProgressItemState.Default, this);
                }
            }

            if (_player != null)
            {
                _player.transform.position = _checkpointProgress.resumePosition.ToVector3();
            }
        }

        // ---------------------------------------------------------------
        // Три "финальных" исхода уровня - единственные точки записи в MetaProgress
        // ---------------------------------------------------------------

        /// <summary>Уровень пройден. Инвентарь и результат мержатся в мета-прогресс.</summary>
        public void CompleteLevel(int finalRating)
        {
            var meta = SaveManager.Instance.MetaProgress;

            MergeInventoryIntoMeta(meta, _currentProgress.currentInventory);
            meta.GetOrCreateLevelRecord(_currentLevelID).RegisterCompletion(finalRating);
            meta.endingSceneProgress = Mathf.Clamp01(meta.endingSceneProgress + RatingToEndingProgress(finalRating));

            SaveManager.Instance.SaveMetaProgress();
        }

        /// <summary>Выход из уровня после смерти (не воскрешение, а именно выход через
        /// экран смерти). Даёт маленький инкремент прогресса финальной сцены и
        /// НЕ сохраняет ничего остального.</summary>
        public void ExitAfterDeath()
        {
            var meta = SaveManager.Instance.MetaProgress;
            meta.endingSceneProgress = Mathf.Clamp01(meta.endingSceneProgress + smallDeathExitIncrement);
            SaveManager.Instance.SaveMetaProgress();

            // TODO: загрузка сцены меню/хаба через вашу систему загрузки сцен.
        }

        /// <summary>Abandon из меню паузы. Полный сброс - мета-прогресс не трогаем совсем.</summary>
        public void AbandonLevel()
        {
            if (showDebugs) Debug.Log("Level abandoned, no progress saved.");
            // Намеренно ничего не пишем в SaveManager.

            // TODO: загрузка сцены меню/хаба через вашу систему загрузки сцен.
        }

        // ---------------------------------------------------------------
        // Вспомогательное
        // ---------------------------------------------------------------

        private void SeedInventoryFromMeta()
        {
            var meta = SaveManager.Instance.MetaProgress;
            _currentProgress.currentInventory = new SavedEntityInventory
            {
                EntityID = _player != null ? _player.GetID : string.Empty,
                EntityEquipmentIDs = meta.unlockedWeapons.ToList(),
                EntityItemIDs = Array.Empty<string>(),
                EntityItemsCount = Array.Empty<int>()
            };
        }

        private void MergeInventoryIntoMeta(MetaProgressData meta, SavedEntityInventory inventory)
        {
            if (inventory == null) return;

            // TODO: заменить на реальную классификацию через DataManager
            // (оружие / чертежи костюмов / чертежи галереи / детали).
            // Ниже - временная упрощённая логика, чтобы поток данных был рабочим.
            if (inventory.EntityEquipmentIDs != null)
            {
                foreach (var id in inventory.EntityEquipmentIDs)
                {
                    meta.unlockedWeapons.Add(id);
                }
            }

            if (inventory.EntityItemIDs != null)
            {
                meta.partsCount += inventory.EntityItemIDs.Length;
            }
        }

        private float RatingToEndingProgress(int rating)
        {
            // TODO: заменить на реальную дизайн-кривую.
            return rating * 0.01f;
        }
    }
}