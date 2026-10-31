using System.Collections.Generic;
using System.Linq;
using Arcatech.Items;
using Arcatech.Managers;
using Arcatech.SaveSystem;
using Arcatech.Units;
using KBCore.Refs;
using SpankyBoy.JuiceUI.Free;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.UI
{
    /// <summary>
    /// Меню «Мастерская» (галерея): две вкладки, «Ловушки» и «Костюмы», со списками панелей.
    /// Прототип: записи ловушек и костюмов задаются списками в инспекторе.
    /// Состояние панели берётся из мета-прогресса (найден / разблокирован), разблокировка списывает детали.
    ///
    /// Контроллер - вьюха инвентаря Теилс (IUnitInventoryView): если Теилс на сцене, выбор костюма
    /// уходит в её EntityInventoryComponent событием ViewChangedInventory.
    /// В любом случае выбор пишется в сейв, откуда его берёт инвентарь при загрузке.
    /// </summary>
    public class GalleryMenuController : ValidatedMonoBehaviour, IUnitInventoryView
    {
        [Header("Данные")]
        [SerializeField] private List<GalleryEntry> trapEntries = new();
        [SerializeField] private List<GalleryEntry> costumeEntries = new();

        [Header("UI")]
        [SerializeField] private GalleryItemPanel panelPrefab;
        [Tooltip("Содержимое вкладки «Ловушки» целиком (включается и выключается при переключении)")]
        [SerializeField] private GameObject trapsTab;
        [Tooltip("Куда создаются панели ловушек (Content списка)")]
        [SerializeField] private Transform trapsListRoot;
        [SerializeField] private GameObject costumesTab;
        [SerializeField] private Transform costumesListRoot;
        [Tooltip("Необязательно: счётчик собранных деталей")]
        [SerializeField] private TMP_Text partsCounterText;

        [Header("Костюм")]
        [Tooltip("Необязательно: инвентарь Теилс на этой сцене. Пусто - ищется объект с тегом Player; нет - костюм только пишется в сейв.")]
        [SerializeField] private EntityInventoryComponent playerInventoryRef;

        private readonly List<GalleryItemPanel> _trapPanels = new();
        private readonly List<GalleryItemPanel> _costumePanels = new();
        private bool _built;

        #region IUnitInventoryView

        public event UnityAction<InventoryChangeType, Item, int> ViewChangedInventory;

        public void RefreshView(InventoryChangeNotification notification)
        {
            // Инвентарь Теилс изменился (в том числе не из меню): обновляем плашки.
            if (_built) RefreshAll();
        }

        #endregion

        private void Start()
        {
            ConnectToTeils();

            BuildPanels(trapEntries, trapsListRoot, false, _trapPanels);
            BuildPanels(costumeEntries, costumesListRoot, true, _costumePanels);
            _built = true;

            UI_ShowTraps();
            RefreshAll();
        }

        private void OnEnable()
        {
            if (_built) RefreshAll();
        }

        private void OnDestroy()
        {
            _built = false;
            Unsubscribe(_trapPanels);
            Unsubscribe(_costumePanels);
        }

        private void ConnectToTeils()
        {
            if (playerInventoryRef == null)
            {
                playerInventoryRef = FindObjectsByType<EntityInventoryComponent>(FindObjectsSortMode.None)
                    .FirstOrDefault(i => i.CompareTag("Player"));
            }

            // Компонент сам не знает про вьюхи вне своего объекта - регистрируем контроллер вручную.
            if (playerInventoryRef != null) playerInventoryRef.SetModelView(this);
        }

        #region кнопки интерфейса

        public void UI_BackButton()
        {
            GameManager.Instance.OnReturnToMain();
        }

        public void UI_ShowTraps() => ShowTab(true);
        public void UI_ShowCostumes() => ShowTab(false);

        #endregion

        private void ShowTab(bool traps)
        {
            if (trapsTab != null) trapsTab.SetActive(traps);
            if (costumesTab != null) costumesTab.SetActive(!traps);
        }

        #region панели

        private void BuildPanels(List<GalleryEntry> entries, Transform root, bool isCostume, List<GalleryItemPanel> into)
        {
            if (root == null || panelPrefab == null)
            {
                Debug.LogWarning($"{nameof(GalleryMenuController)}: не назначен список или префаб панели " +
                                 $"({(isCostume ? "костюмы" : "ловушки")}).", this);
                return;
            }

            foreach (var entry in entries)
            {
                if (entry == null) continue;
                var panel = Instantiate(panelPrefab, root);
                panel.Setup(entry, isCostume);
                panel.UnlockClicked += OnUnlockClicked;
                panel.ActionClicked += OnActionClicked;
                into.Add(panel);
            }
        }

        private void Unsubscribe(List<GalleryItemPanel> panels)
        {
            foreach (var panel in panels)
            {
                if (panel == null) continue;
                panel.UnlockClicked -= OnUnlockClicked;
                panel.ActionClicked -= OnActionClicked;
            }
        }

        private void RefreshAll()
        {
            var meta = SaveManager.Instance.MetaProgress;
            if (partsCounterText != null) partsCounterText.text = meta.partsCount.ToString();

            foreach (var panel in _trapPanels) RefreshPanel(panel, meta);
            foreach (var panel in _costumePanels) RefreshPanel(panel, meta);
        }

        private static void RefreshPanel(GalleryItemPanel panel, MetaProgressData meta)
        {
            var entry = panel.Entry;
            var state = GetState(entry, panel.IsCostume, meta);
            bool equipped = panel.IsCostume
                            && entry.costume != null
                            && !string.IsNullOrEmpty(meta.equippedCostumeID)
                            && meta.equippedCostumeID == entry.costume.ID;
            panel.Refresh(state, meta.partsCount, equipped);
        }

        private static GalleryEntryState GetState(GalleryEntry entry, bool isCostume, MetaProgressData meta)
        {
            var id = entry.BlueprintId;
            // Костюм без чертежа (стартовый) открыт с начала игры.
            if (string.IsNullOrEmpty(id)) return isCostume && entry.costume != null
                ? GalleryEntryState.Unlocked
                : GalleryEntryState.Hidden;

            var unlocked = isCostume ? meta.unlockedCostumeBlueprints : meta.unlockedGalleryBlueprints;
            if (unlocked.Contains(id)) return GalleryEntryState.Unlocked;

            var found = isCostume ? meta.foundCostumeBlueprints : meta.foundGalleryBlueprints;
            return found.Contains(id) ? GalleryEntryState.Found : GalleryEntryState.Hidden;
        }

        #endregion

        #region действия

        private void OnUnlockClicked(GalleryItemPanel panel)
        {
            var meta = SaveManager.Instance.MetaProgress;
            if (!meta.TryUnlockBlueprint(panel.Entry.BlueprintId, panel.Entry.unlockCost))
            {
                // не хватает деталей или чертёж уже разблокирован: просто обновляем вид
                RefreshAll();
                return;
            }

            // списание деталей должно сразу попасть на диск, иначе перезапуск вернёт их обратно
            SaveManager.Instance.SaveMetaProgress();
            RefreshAll();
        }

        private void OnActionClicked(GalleryItemPanel panel)
        {
            if (panel.IsCostume) EquipCostume(panel.Entry);
            else ExploreTrap(panel.Entry);
        }

        private void EquipCostume(GalleryEntry entry)
        {
            if (entry.costume == null)
            {
                Debug.LogWarning($"{nameof(GalleryMenuController)}: у записи «{entry.ResolveTitle()}» не задан костюм.", this);
                return;
            }

            // 1. Сейв: главное меню и старт уровня берут костюм отсюда.
            var meta = SaveManager.Instance.MetaProgress;
            meta.equippedCostumeID = entry.costume.ID;
            SaveManager.Instance.SaveMetaProgress();

            // 2. Теилс на сцене: просим её инвентарь надеть костюм сразу.
            bool equippedNow = RequestEquip(entry.costume);
            Debug.Log(equippedNow
                ? $"[Мастерская] Костюм «{entry.ResolveTitle()}» надет и записан в сейв."
                : $"[Мастерская] Костюм «{entry.ResolveTitle()}» записан в сейв (Теилс на сцене нет).");
            RefreshAll();
        }

        /// <returns>true, если запрос ушёл в инвентарь Теилс.</returns>
        private bool RequestEquip(EquipSO config)
        {
            // Нет подписчика (Теилс нет на сцене или её инвентарь выключен) - событие некому обработать.
            if (playerInventoryRef == null || ViewChangedInventory == null) return false;

            // Предмету нужен владелец: экипировка создаётся под объектом Теилс.
            var owner = playerInventoryRef.GetComponent<BaseGameEntityComponent>();
            var item = DataManager.Instance.MakeItem(config, owner);
            if (item == null) return false;

            ViewChangedInventory.Invoke(InventoryChangeType.Equip, item, 1);
            return true;
        }

        private void ExploreTrap(GalleryEntry entry)
        {
            // Заглушка: просмотр сцены ловушки (Scene Viewer) подключим позже.
            var settings = SaveManager.Instance.Settings;
            Debug.Log($"[Мастерская] Запуск анимации ловушки «{entry.ResolveTitle()}» " +
                      $"(режим контента: {settings.contentMode}, режим стримера: {settings.streamerMode})");
        }

        #endregion

        protected override void OnValidate()
        {
            base.OnValidate();
            WarnOnWrongCategory(trapEntries, ItemCategory.GalleryBlueprint);
            WarnOnWrongCategory(costumeEntries, ItemCategory.CostumeBlueprint);
        }

        private void WarnOnWrongCategory(List<GalleryEntry> entries, ItemCategory expected)
        {
            if (entries == null) return;
            foreach (var entry in entries)
            {
                if (entry?.blueprint != null && entry.blueprint.Category != expected)
                    Debug.LogWarning($"{nameof(GalleryMenuController)}: чертёж «{entry.blueprint.name}» " +
                                     $"имеет категорию {entry.blueprint.Category}, а здесь ожидается {expected}.", this);
            }
        }
    }

}
