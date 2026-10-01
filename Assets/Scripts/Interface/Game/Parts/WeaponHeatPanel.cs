using Arcatech.Usables;
using DG.Tweening;
using KBCore.Refs;
using SpankyBoy.JuiceUI.Free;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.UI
{
    /// <summary>
    /// Панель перегрева оружия дальнего боя. Управляет вложенной полоской (StatBarContainerUIScript):
    /// - скрыта по умолчанию, показывается, пока у игрока экипировано оружие с перегревом
    ///   (WeaponHeatComponent.Active != null), прячется, когда такого оружия нет;
    /// - выводит в полоску нагрев и максимум активного оружия (у каждого оружия свой максимум);
    /// - сообщает о переходе в перегрев / из него (маркер и UnityEvent для анимаций и звука).
    ///
    /// Сам компонент должен висеть на объекте, который всегда активен: скрывается только content,
    /// иначе Awake / Start у скрытой панели не вызвались бы.
    /// </summary>
    public class WeaponHeatPanel : ValidatedMonoBehaviour
    {
        [Header("Prefab parts")]
        [SerializeField, Child] private StatBarContainerUIScript bar;

        [SerializeField, Tooltip("Что показывать и прятать. Не задано - сама полоска")]
        private GameObject content;

        [SerializeField, Tooltip("Необязательно: анимация появления / скрытия (как у других панелей)")]
        private PanelAnimator animator;

        [Header("Source")]
        [SerializeField, Tooltip("Найти игрока по тегу Player и взять у него WeaponHeatComponent. " +
                                 "Выключено: вызвать Bind() из кода")]
        private bool autoFindPlayer = true;

        [Header("Bar look")]
        [SerializeField, Tooltip("Positive - нагрев растет, Negative - нагрев падает (вспышка при больших скачках)")]
        private ColorSet colors;

        [SerializeField] private Ease easeMethod = Ease.Linear;
        [SerializeField, Min(0f)] private float fillTime = 0.3f;
        [SerializeField, Range(0f, 1f), Tooltip("Доля максимума: скачок больше вызывает вспышку фона полоски")]
        private float flashThreshold = 0.2f;

        [Header("Overheat feedback")]
        [SerializeField, Tooltip("Необязательно: включается на время перегрева (замок, надпись и т.п.)")]
        private GameObject overheatedMarker;

        [SerializeField, Tooltip("true - перегрев начался, false - закончился")]
        private UnityEvent<bool> overheatChanged;

        private WeaponHeatComponent _heat;
        private SerializedOverheatStrategy _shownWeapon;
        private float _lastHeat;
        private bool _visible;
        private bool _overheated;

        public bool IsVisible => _visible;

        private void Awake()
        {
            if (content == null && bar != null) content = bar.gameObject;

            if (content == gameObject)
            {
                Debug.LogWarning($"{name}: content совпадает с объектом панели - при скрытии она перестанет " +
                                 "получать обновления. Назначьте дочерний объект", this);
            }

            if (bar != null)
            {
                bar.SetColors(colors).
                    SetEaseMethod(easeMethod).SetFillTime(fillTime).SetBrightGlowAT(flashThreshold);
            }
            else
            {
                Debug.LogError($"{name}: StatBarContainerUIScript не найден среди дочерних объектов", this);
            }

            if (overheatedMarker != null) overheatedMarker.SetActive(false);
            SetVisible(false, true); // по умолчанию скрыта
        }

        private void Start()
        {
            if (_heat != null || !autoFindPlayer) return;

            var player = GameObject.FindWithTag("Player");
            var heat = player != null ? WeaponHeatComponent.Find(player.transform) : null;
            if (heat == null)
            {
                Debug.LogWarning($"{name}: у игрока нет WeaponHeatComponent, панель перегрева остается скрытой", this);
                return;
            }

            Bind(heat);
        }

        private void OnDestroy() => Unbind();

        /// <summary>Подключает панель к нагреву игрока. null - отключает и скрывает</summary>
        public void Bind(WeaponHeatComponent heat)
        {
            if (_heat == heat)
            {
                if (_heat != null) Refresh();
                return;
            }

            Unbind();
            _heat = heat;

            if (_heat == null)
            {
                SetVisible(false);
                return;
            }

            _lastHeat = _heat.Heat;
            _heat.Changed += Refresh;
            _heat.DebugLog($"WeaponHeatPanel '{name}' bound");
            Refresh(); // оружие могло быть экипировано до создания панели
        }

        private void Unbind()
        {
            if (_heat != null) _heat.Changed -= Refresh;
            _heat = null;
        }

        private void Refresh()
        {
            var weapon = _heat.Active;
            if (weapon == null || bar == null)
            {
                SetVisible(false); // оружия с перегревом нет
                SetOverheated(false);
                _shownWeapon = null;
                return;
            }

            bool justShown = SetVisible(true);

            float max = weapon.maxHeat;
            float current = _heat.Heat;

            // при показе и смене оружия вспышку не нужна: дельта считается от нуля изменения
            bool weaponChanged = weapon != _shownWeapon;
            float delta = justShown || weaponChanged ? 0f : current - _lastHeat;

            _shownWeapon = weapon;
            _lastHeat = current;

            bar.UpdateValue(current, max, delta);
            SetOverheated(_heat.IsOverheated);
        }

        private void SetOverheated(bool value)
        {
            if (_overheated == value) return;
            _overheated = value;

            if (overheatedMarker != null) overheatedMarker.SetActive(value);
            overheatChanged?.Invoke(value);
            if (_heat != null) _heat.DebugLog($"WeaponHeatPanel: overheated marker -> {value}");
        }

        /// <returns>true, если панель только что стала видимой</returns>
        private bool SetVisible(bool visible, bool instant = false)
        {
            if (content == null || _visible == visible && !instant) return false;

            bool changed = _visible != visible;
            _visible = visible;

            if (visible)
            {
                content.SetActive(true);
                if (animator != null) animator.Show();
            }
            else if (instant || animator == null)
            {
                content.SetActive(false);
            }
            else if (content.activeSelf)
            {
                animator.Hide();
            }

            if (changed && _heat != null) _heat.DebugLog($"WeaponHeatPanel: visible -> {visible}");
            return changed && visible;
        }
    }
}
