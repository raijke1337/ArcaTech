using System;
using System.Collections.Generic;
using System.Linq;
using Arcatech.Items;
using Arcatech.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Arcatech.SaveSystem
{
    /// <summary>
    /// Не персистентен: создаётся заново на каждой игровой сцене (уровне).
    /// Владеет геймплейным прогрессом уровня.
    ///
    /// Прототип: откат к чекпоинту = перезагрузка сцены + применение сохранённого
    /// чекпоинта к объектам. Тот же путь работает для «Продолжить» из главного меню.
    ///
    /// Исходы уровня:
    ///   - смерть              - тратится жизнь, чекпоинт с новым числом жизней сразу пишется на диск
    ///   - ReturnToCheckpoint  - «Попробовать ещё раз» (если жизни остались)
    ///   - RestartLevelFromScratch - «Заново»: чекпоинт стирается, 9 жизней
    ///   - LeaveLevelKeepingCheckpoint - «Выйти» из паузы: чекпоинт остаётся для «Продолжить»
    ///   - ExitAfterDeath      - выход с экрана смерти: +прогресс финальной сцены, чекпоинт стирается
    ///   - CompleteLevel       - уровень пройден: рейтинг, находки уходят в мета-прогресс
    /// </summary>
    public class LevelProgressController : GenericLazySingleton<LevelProgressController>, ILevelProgressContext
    {
        [SerializeField] private bool showDebugs = false;

        [Header("Lives")]
        [SerializeField, Min(1)] private int maxLives = 9;

        [Tooltip("Если на диске есть чекпоинт этого уровня - загрузиться с него. " +
                 "Выключите, чтобы в редакторе всегда начинать уровень с начала.")]
        [SerializeField] private bool resumeFromSavedCheckpoint = true;

        [Header("Rating (доля от 0 до 1)")]
        [SerializeField, Range(0, 1)] private float gradeS = 0.9f;
        [SerializeField, Range(0, 1)] private float gradeA = 0.75f;
        [SerializeField, Range(0, 1)] private float gradeB = 0.5f;

        [Header("Ending scene progress")]
        [SerializeField, Range(0, 1)] private float endingProgressPerCompletion = 0.01f;
        [SerializeField, Range(0, 1)] private float endingProgressPerDeathExit = 0.01f;

        private string _currentLevelID;
        public string CurrentLevelId { get { EnsureInitialized(); return _currentLevelID; } }

        private LevelProgressData _currentProgress;
        private LevelProgressData _checkpointProgress;

        private readonly Dictionary<string, ISavedProgressItem> _trackedItems = new();
        // Все объекты прогресса уровня, включая уже уничтоженные - для подсчёта рейтинга.
        private readonly Dictionary<string, ProgressItemKind> _knownItems = new();
        // Применение состояний откладывается до LateUpdate: SetActive(false) внутри
        // OnEnable того же объекта Unity не разрешает.
        private readonly List<ISavedProgressItem> _pendingApply = new();

        private BaseGameEntityComponent _player;
        private bool _initialized;
        private bool _isGameLevel;
        private bool _resumedFromCheckpoint;
        private bool _deathHandled;
        private bool _levelFinished;

        public int MaxLives => maxLives;
        public int LivesRemaining { get { EnsureInitialized(); return _currentProgress?.livesRemaining ?? maxLives; } }
        public bool CanRetry => LivesRemaining > 0;
        public bool ResumedFromCheckpoint { get { EnsureInitialized(); return _resumedFromCheckpoint; } }

        /// <summary>Срабатывает при фиксации нового чекпоинта (например, для UI-фидбека).</summary>
        public event Action OnCheckpointReached;
        public event Action<int> OnLivesChanged;
        public event Action<LevelResult> OnLevelCompleted;

        // ---------------------------------------------------------------
        // Инициализация
        // ---------------------------------------------------------------

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Debug.LogWarning($"Second LevelProgressController on {name} - destroyed.");
                Destroy(this);
                return;
            }
            instance = this;
        }

        private void Start()
        {
            EnsureInitialized(gameObject.scene);
            if (_resumedFromCheckpoint) PlacePlayerAtResumePoint();
        }

        private void OnDestroy()
        {
            if (_player != null) _player.AnnounceDead.RemoveListener(OnPlayerAnnounceDead);
        }

        private void EnsureInitialized(Scene sceneHint = default)
        {
            if (_initialized) return;
            _initialized = true;

            var scene = sceneHint.IsValid() ? sceneHint : SceneManager.GetActiveScene();
            _currentLevelID = scene.name;

            _player = FindObjectsByType<BaseGameEntityComponent>(FindObjectsSortMode.None)
                .FirstOrDefault(t => t.CompareTag("Player") && t.gameObject.scene == scene);

            // Контроллер может создаться и в меню (например, если кто-то спросил
            // стартовый инвентарь). Тогда он ничего не пишет на диск.
            var container = DataManager.Instance.GetSceneContainer(scene.buildIndex);
            _isGameLevel = container != null
                ? container.LevelType == LevelType.Game
                : _player != null;
            if (container == null && showDebugs)
                Debug.LogWarning($"LevelProgressController: no SceneContainer for scene {scene.name}. " +
                                 $"Treated as {(_isGameLevel ? "a game level (player found)" : "a menu")}.");

            if (!_isGameLevel)
            {
                _currentProgress = new LevelProgressData { levelID = _currentLevelID, livesRemaining = maxLives };
                _checkpointProgress = new LevelProgressData(_currentProgress);
                return;
            }

            if (_player != null)
                _player.AnnounceDead.AddListener(OnPlayerAnnounceDead);
            else
                Debug.LogWarning("LevelProgressController: no Player found on scene.");

            ScanLevelItems(scene);

            if (resumeFromSavedCheckpoint
                && SaveManager.Instance.TryGetCheckpoint(out var saved)
                && saved.levelID == _currentLevelID)
            {
                _checkpointProgress = saved;
                _resumedFromCheckpoint = true;
                if (showDebugs) Debug.Log($"Resuming {_currentLevelID} from checkpoint, lives: {saved.livesRemaining}");
            }
            else
            {
                // Начало уровня - неявный чекпоинт.
                _checkpointProgress = new LevelProgressData
                {
                    levelID = _currentLevelID,
                    sceneBuildIndex = scene.buildIndex,
                    livesRemaining = maxLives,
                    resumePosition = (_player != null ? _player.transform.position : Vector3.zero).ToSerializable()
                };
                SaveManager.Instance.SaveCheckpoint(_checkpointProgress);
                if (showDebugs) Debug.Log($"Fresh start of {_currentLevelID}.");
            }

            _currentProgress = new LevelProgressData(_checkpointProgress);
            ApplyFlags(_checkpointProgress.setFlags);
        }

        /// <summary>Находит все объекты прогресса уровня, включая выключенные,
        /// чтобы рейтинг знал общее число врагов и предметов.</summary>
        private void ScanLevelItems(Scene scene)
        {
            var owners = new Dictionary<string, SavedProgressItemBase>();
            foreach (var item in FindObjectsByType<SavedProgressItemBase>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (item.gameObject.scene != scene || !item.IsTracked) continue;

                var id = item.SavedItemID;
                if (owners.TryGetValue(id, out var other) && other != item)
                {
                    Debug.LogError($"Duplicate EntityID {id}: '{other.name}' and '{item.name}'. " +
                                   "Use 'Regenerate ID' in the EntityID context menu on one of them.", item);
                    continue;
                }

                owners[id] = item;
                _knownItems[id] = item.Kind;
            }
        }

        private void PlacePlayerAtResumePoint()
        {
            if (_player == null || _checkpointProgress.resumePosition == null) return;

            var pos = _checkpointProgress.resumePosition.ToVector3();
            if (_player.TryGetComponent(out Rigidbody rb)) rb.position = pos;
            _player.transform.position = pos;
            Physics.SyncTransforms();
        }

        private void LateUpdate()
        {
            if (_pendingApply.Count == 0) return;

            // Копия: применение состояния может выключить объект и вызвать Unregister.
            var batch = _pendingApply.ToArray();
            _pendingApply.Clear();
            foreach (var item in batch)
            {
                if (item is UnityEngine.Object obj && obj == null) continue; // уже уничтожен
                if (_currentProgress.progressItemStates.TryGetValue(item.SavedItemID, out var state))
                    item.ApplySaveState(state, this);
            }
        }

        // ---------------------------------------------------------------
        // Регистрация объектов прогресса
        // ---------------------------------------------------------------

        public void Register(ISavedProgressItem item)
        {
            if (item == null || string.IsNullOrEmpty(item.SavedItemID)) return;
            EnsureInitialized(item is Component c ? c.gameObject.scene : default);
            if (!_isGameLevel) return;

            _trackedItems[item.SavedItemID] = item;
            _knownItems.TryAdd(item.SavedItemID, item.Kind);

            // Объект мог включиться позже (например, при открытии блока уровня) -
            // приводим его к нужному состоянию.
            if (_currentProgress.progressItemStates.TryGetValue(item.SavedItemID, out var state)
                && state != ProgressItemState.Default)
            {
                _pendingApply.Add(item);
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
            EnsureInitialized(item is Component c ? c.gameObject.scene : default);
            if (!_isGameLevel || _levelFinished) return;

            if (showDebugs) Debug.Log($"Recording state {item.ReadItemState} for {item.Name} ({item.SavedItemID})");
            _currentProgress.progressItemStates[item.SavedItemID] = item.ReadItemState;
            _knownItems.TryAdd(item.SavedItemID, item.Kind);
        }

        /// <summary>Вызывается инвентарём игрока при каждом изменении.</summary>
        public void RecordInventory(SavedEntityInventory inventory)
        {
            EnsureInitialized();
            if (!_isGameLevel || _levelFinished || inventory == null) return;
            _currentProgress.currentInventory = new SavedEntityInventory(inventory);
        }

        /// <summary>Подобранная на уровне экипировка - отдельно, т.к. она может
        /// вытеснить из слота другую и пропасть из инвентаря.</summary>
        public void RecordFoundEquipment(string equipmentID)
        {
            EnsureInitialized();
            if (!_isGameLevel || _levelFinished || string.IsNullOrEmpty(equipmentID)) return;
            _currentProgress.foundEquipmentIDs.Add(equipmentID);
        }

        /// <summary>С чем игрок выходит на уровень. null - использовать пресет по умолчанию.
        /// Загрузка с чекпоинта - инвентарь из чекпоинта; новый старт - экипировка,
        /// назначенная в интерфейсе «Персонаж».</summary>
        public SavedEntityInventory GetStartingInventory(Scene sceneHint = default)
        {
            EnsureInitialized(sceneHint);

            if (_isGameLevel && _currentProgress.currentInventory != null)
                return new SavedEntityInventory(_currentProgress.currentInventory);

            var loadout = SaveManager.Instance.MetaProgress.equippedLoadout;
            if (loadout == null || loadout.Count == 0) return null;

            return new SavedEntityInventory
            {
                EntityID = _player != null ? _player.GetID : string.Empty,
                EntityEquipmentIDs = new List<string>(loadout)
            };
        }

        public void OnCheckPointReached(CheckpointTrigger trigger)
        {
            EnsureInitialized(trigger.gameObject.scene);
            if (!_isGameLevel || _levelFinished) return;
            if (showDebugs) Debug.Log($"Checkpoint reached: {trigger.name}");

            _currentProgress.setFlags = CaptureFlags();
            _checkpointProgress = new LevelProgressData(_currentProgress)
            {
                resumePosition = trigger.transform.position.ToSerializable()
            };
            SaveManager.Instance.SaveCheckpoint(_checkpointProgress);

            OnCheckpointReached?.Invoke();
        }

        // ---------------------------------------------------------------
        // Смерть и откат
        // ---------------------------------------------------------------

        private void OnPlayerAnnounceDead(BaseGameEntityComponent entity)
        {
            if (entity.EntityAlive) return;           // событие также стреляет при воскрешении
            if (_deathHandled || _levelFinished) return; // SetKilled может прийти несколько раз
            _deathHandled = true;

            var lives = Mathf.Max(0, _checkpointProgress.livesRemaining - 1);
            _checkpointProgress.livesRemaining = lives;
            _currentProgress.livesRemaining = lives;

            // Пишем сразу, чтобы жизнь нельзя было вернуть перезапуском игры.
            if (lives > 0) SaveManager.Instance.SaveCheckpoint(_checkpointProgress);
            else SaveManager.Instance.ClearCheckpoint();

            if (showDebugs) Debug.Log($"Player died. Lives left: {lives}");
            OnLivesChanged?.Invoke(lives);
            GameInterfaceManager.Instance.ShowPlayerDeadMenu(lives > 0);
        }

        /// <summary>«Попробовать ещё раз»: перезагрузка сцены, загрузка с последнего чекпоинта.</summary>
        public void ReturnToCheckpoint()
        {
            EnsureInitialized();
            if (!CanRetry)
            {
                Debug.LogWarning("ReturnToCheckpoint: no lives left.");
                return;
            }
            ReloadLevel();
        }

        /// <summary>«Заново»: уровень с начала, 9 жизней, экипировка из интерфейса «Персонаж».</summary>
        public void RestartLevelFromScratch()
        {
            EnsureInitialized();
            SaveManager.Instance.ClearCheckpoint();
            ReloadLevel();
        }

        private void ReloadLevel()
        {
            _levelFinished = true; // объекты выгружаемой сцены больше ничего не пишут
            var index = SceneManager.GetActiveScene().buildIndex;
            if (index >= 0) SceneManager.LoadScene(index);
            else SceneManager.LoadScene(_currentLevelID);
        }

        /// <summary>«Выйти в главное меню» из паузы. Последний чекпоинт уже на диске -
        /// с него продолжит кнопка «Продолжить». Прогресс после чекпоинта теряется.</summary>
        public void LeaveLevelKeepingCheckpoint()
        {
            EnsureInitialized();
            _levelFinished = true;
            if (showDebugs) Debug.Log("Leaving level, checkpoint kept.");
        }

        /// <summary>Выход с экрана смерти. Небольшой инкремент финальной сцены,
        /// остальной прогресс уровня не сохраняется.</summary>
        public void ExitAfterDeath()
        {
            EnsureInitialized();
            if (!_isGameLevel || _levelFinished) return;
            _levelFinished = true;

            var meta = SaveManager.Instance.MetaProgress;
            meta.endingSceneProgress = Mathf.Clamp01(meta.endingSceneProgress + endingProgressPerDeathExit);
            SaveManager.Instance.SaveMetaProgress();
            SaveManager.Instance.ClearCheckpoint();
        }

        // ---------------------------------------------------------------
        // Прохождение уровня
        // ---------------------------------------------------------------

        /// <summary>Уровень пройден: считает рейтинг, переносит находки в мета-прогресс,
        /// открывает следующий уровень, стирает чекпоинт.</summary>
        public LevelResult CompleteLevel()
        {
            EnsureInitialized();
            if (!_isGameLevel || _levelFinished) return default;
            _levelFinished = true;

            var result = CalculateResult();
            var meta = SaveManager.Instance.MetaProgress;
            result.isNewBest = !meta.TryGetLevelRecord(_currentLevelID, out var previous)
                               || !previous.isCompleted
                               || result.score > previous.bestScore;

            MergeFindingsIntoMeta(meta);
            meta.GetOrCreateLevelRecord(_currentLevelID).RegisterCompletion(result);
            UnlockNextLevel(meta);
            meta.endingSceneProgress = Mathf.Clamp01(meta.endingSceneProgress + endingProgressPerCompletion);

            SaveManager.Instance.SaveMetaProgress();
            SaveManager.Instance.ClearCheckpoint();

            if (showDebugs) Debug.Log($"Level {_currentLevelID} completed: {result}");
            OnLevelCompleted?.Invoke(result);
            return result;
        }

        /// <summary>Текущие доли для UI (интерфейс «Уровень» во время игры).</summary>
        public LevelResult CalculateResult()
        {
            EnsureInitialized();
            var result = new LevelResult
            {
                livesRemaining = _currentProgress.livesRemaining,
                maxLives = maxLives,
                livesShare = Mathf.Clamp01((float)_currentProgress.livesRemaining / maxLives),
                killsShare = CompletedShare(ProgressItemKind.Enemy),
                itemsShare = CompletedShare(ProgressItemKind.Collectible)
            };
            (result.enemiesKilled, result.enemiesTotal) = CountCompleted(ProgressItemKind.Enemy);
            (result.itemsCollected, result.itemsTotal) = CountCompleted(ProgressItemKind.Collectible);
            (result.secretsFound, result.secretsTotal) = CountCompleted(ProgressItemKind.Secret);
            result.score = (result.livesShare + result.killsShare + result.itemsShare) / 3f;
            result.grade = result.score >= gradeS ? LevelGrade.S
                : result.score >= gradeA ? LevelGrade.A
                : result.score >= gradeB ? LevelGrade.B
                : LevelGrade.C;
            return result;
        }

        private float CompletedShare(ProgressItemKind kind)
        {
            var (done, total) = CountCompleted(kind);
            return total == 0 ? 1f : Mathf.Clamp01((float)done / total);
        }

        private (int done, int total) CountCompleted(ProgressItemKind kind)
        {
            int total = _knownItems.Count(p => p.Value == kind);
            int done = _currentProgress.progressItemStates.Count(p =>
                p.Value == ProgressItemState.Completed
                && _knownItems.TryGetValue(p.Key, out var k) && k == kind);
            return (Mathf.Min(done, total), total);
        }

        /// <summary>Название уровня из SceneContainer (Description.Title), иначе имя сцены.</summary>
        public string CurrentLevelTitle
        {
            get
            {
                EnsureInitialized();
                var container = DataManager.Instance.GetSceneContainer(SceneManager.GetActiveScene().buildIndex);
                var title = container != null && container.Description != null ? container.Description.Title : null;
                return string.IsNullOrWhiteSpace(title) ? _currentLevelID : title;
            }
        }

        private void MergeFindingsIntoMeta(MetaProgressData meta)
        {
            var inventory = _currentProgress.currentInventory;

            foreach (var id in _currentProgress.foundEquipmentIDs) meta.unlockedWeapons.Add(id);
            if (inventory?.EntityEquipmentIDs != null)
                foreach (var id in inventory.EntityEquipmentIDs) meta.unlockedWeapons.Add(id);

            if (inventory?.EntityItemIDs == null || inventory.EntityItemsCount == null) return;

            for (int i = 0; i < inventory.EntityItemIDs.Length && i < inventory.EntityItemsCount.Length; i++)
            {
                var id = inventory.EntityItemIDs[i];
                if (!DataManager.Instance.TryGetItemConfig(id, out var config))
                {
                    Debug.LogWarning($"CompleteLevel: unknown item {id} - not merged into meta progress.");
                    continue;
                }

                switch (config.Category)
                {
                    case ItemCategory.Part:
                        meta.partsCount += inventory.EntityItemsCount[i];
                        break;
                    case ItemCategory.CostumeBlueprint:
                        if (!meta.unlockedCostumeBlueprints.Contains(id)) meta.foundCostumeBlueprints.Add(id);
                        break;
                    case ItemCategory.GalleryBlueprint:
                        if (!meta.unlockedGalleryBlueprints.Contains(id)) meta.foundGalleryBlueprints.Add(id);
                        break;
                    // LevelOnly (ключи и т.п.) в мета-прогресс не переносятся.
                }
            }
        }

        private void UnlockNextLevel(MetaProgressData meta)
        {
            var container = DataManager.Instance.GetSceneContainer(SceneManager.GetActiveScene().buildIndex);
            if (container == null || container.NextLevel == null) return;
            meta.GetOrCreateLevelRecord(DataManager.GetLevelID(container.NextLevel)).isUnlocked = true;
        }

        // ---------------------------------------------------------------
        // Флаги целей (ObjectiveFlagSO хранят состояние в памяти и переживают
        // перезагрузку сцены, поэтому их нужно сбрасывать и сохранять в чекпоинт)
        // ---------------------------------------------------------------

        private static List<string> CaptureFlags() =>
            Resources.FindObjectsOfTypeAll<ObjectiveFlagSO>()
                .Where(f => f.IsSet)
                .Select(f => f.name)
                .ToList();

        private static void ApplyFlags(List<string> setFlags)
        {
            var set = setFlags != null ? new HashSet<string>(setFlags) : new HashSet<string>();
            foreach (var flag in Resources.FindObjectsOfTypeAll<ObjectiveFlagSO>())
            {
                flag.Reset();
                if (set.Contains(flag.name)) flag.Set();
            }
        }
    }
}
