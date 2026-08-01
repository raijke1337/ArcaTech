using Arcatech.Stats;
using AYellowpaper.SerializedCollections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arcatech.UI
{
    public class IconContainerUIScript : MonoBehaviour
    {
        [Header("Icons Display")]
        [SerializeField]
        private SerializedDictionary<ResourceStatType, Sprite> icons;

        [Space]
        [SerializeField]
        private Image usableIcon;

        [Header("Cooldown")]
        [SerializeField]
        private Image usableIconCooldownFill;

        [SerializeField]
        private Image usableIconCooldownBorderFill;

        [SerializeField]
        private Image usableIconReadyGlowBorder;

        [SerializeField]
        private TextMeshProUGUI usableIconCooldownText;

        [Header("Cost")]
        [SerializeField]
        private Image costIcon;

        [SerializeField]
        private TextMeshProUGUI costIconText;

        [Header("Charges")]
        [SerializeField]
        private TextMeshProUGUI chargesText;

        [Header("Hotkey")]
        [SerializeField]
        TextMeshProUGUI hotkeyText;
        [Header("Animation")]
        [SerializeField, Min(0f)]
        private float usePunchStrength = 0.12f;

        [SerializeField, Min(0f)]
        private float usePunchDuration = 0.18f;

        [SerializeField, Min(0f)]
        private float failedUseShakeStrength = 8f;

        [SerializeField, Min(0f)]
        private float failedUseShakeDuration = 0.25f;

        private IActionIconContent content;
        private RectTransform rectTransform;

        private int lastCharges = int.MinValue;
        private float lastCooldown = float.MinValue;
        private bool lastReadyState;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void OnDisable()
        {
            rectTransform.DOKill();
        }

        public IconContainerUIScript WithHotkey(string hkey)
        {
            if (hotkeyText == null)
            {
                return this;
            }

            bool hasHotkey = !string.IsNullOrWhiteSpace(hkey);

            hotkeyText.gameObject.SetActive(hasHotkey);

            if (hasHotkey)
            {
                hotkeyText.text = hkey;
            }

            return this;
        }

        /// <summary>
        /// Привязывает иконку к применению.
        /// Само состояние способности берется из content в Update().
        /// </summary>
        public IconContainerUIScript AssignIcon(IActionIconContent newContent)
        {
            content = newContent;

            if (content == null)
            {
                SetVisible(false);
                return this;
            }

            SetVisible(true);

            usableIcon.sprite = content.Description.Picture;

            (ResourceStatType resourceType, int cost) = content.GetCostDescription;

            if (icons.TryGetValue(resourceType, out Sprite resourceIcon))
            {
                costIcon.sprite = resourceIcon;
                costIcon.enabled = resourceIcon != null;
            }
            else
            {
                Debug.LogWarning(
                    $"[{nameof(IconContainerUIScript)}] Не найдена иконка ресурса типа {resourceType}.",
                    this
                );
                costIcon.enabled = false;
            }
            costIconText.text = cost.ToString();

            // Сбрасываем кэш, чтобы сразу принудительно обновить UI.
            lastCharges = int.MinValue;
            lastCooldown = float.MinValue;

            RefreshVisuals(force: true);
            return this;
        }

        private void Update()
        {
            if (content == null)
            {
                return;
            }

            RefreshVisuals();
        }

        private void RefreshVisuals(bool force = false)
        {
            int currentCharges = Mathf.Max(0, content.CurrentCharges);
            int maxCharges = Mathf.Max(0, content.MaxCharges);

            float maxCooldown = Mathf.Max(0f, content.Cooldown);
            float currentCooldown = Mathf.Clamp(content.CurrentCooldown, 0f, maxCooldown);

            bool usesCharges = maxCharges > 0;
            bool hasCharges = currentCharges > 0;
            bool hasCooldown = maxCooldown > 0f && currentCooldown > 0f;

            // Способность считается готовой, если доступен хотя бы один заряд.
            // Даже если второй заряд сейчас восстанавливается, первый уже можно использовать.
            /*
             * Способность без зарядов готова, если ее cooldown закончился.
             */
            bool isReady = usesCharges
                ? hasCharges
                : !hasCooldown;

            if (!force &&
                currentCharges == lastCharges &&
                Mathf.Approximately(currentCooldown, lastCooldown) &&
                isReady == lastReadyState)
            {
                return;
            }

            lastCharges = currentCharges;
            lastCooldown = currentCooldown;
            lastReadyState = isReady;

            UpdateCharges(currentCharges, maxCharges);
            UpdateCooldown(
                currentCooldown,
                maxCooldown,
                usesCharges,
                hasCharges,
                hasCooldown
            );
            
            UpdateReadyState(isReady);
        }

        private void UpdateCharges(int currentCharges, int maxCharges)
        {
            bool shouldShowCharges = maxCharges > 0;

            chargesText.gameObject.SetActive(shouldShowCharges);

            if (!shouldShowCharges)
            {
                return;
            }

            chargesText.text = $"{currentCharges}/{maxCharges}";
        }

        private void UpdateCooldown(
            float currentCooldown,
            float maxCooldown,
            bool usesCharges,
            bool hasCharges,
            bool hasCooldown)
        {
            float cooldownRemainingNormalized = maxCooldown <= 0f
                ? 0f
                : currentCooldown / maxCooldown;

            float cooldownProgressNormalized = 1f - cooldownRemainingNormalized;

            /*
             * Центральная маска нужна, если способность нельзя применить:
             *
             * - у способности с зарядами нет зарядов;
             * - у способности без зарядов еще идет cooldown.
             */
            bool isUnavailable = usesCharges
                ? !hasCharges
                : hasCooldown;

            usableIconCooldownFill.gameObject.SetActive(isUnavailable && hasCooldown);

            usableIconCooldownFill.fillAmount = isUnavailable
                ? cooldownRemainingNormalized
                : 0f;

            /*
             * Внешний контур cooldown может отображаться даже если один заряд уже готов:
             * игрок увидит, что следующий заряд восстанавливается.
             */
            usableIconCooldownBorderFill.gameObject.SetActive(hasCooldown);

            usableIconCooldownBorderFill.fillAmount = hasCooldown
                ? cooldownProgressNormalized
                : 0f;

            /*
             * Цифровой таймер показываем, только когда применение полностью недоступно.
             */
            bool shouldShowCooldownText = isUnavailable && hasCooldown;

            usableIconCooldownText.gameObject.SetActive(shouldShowCooldownText);

            if (shouldShowCooldownText)
            {
                usableIconCooldownText.text = currentCooldown >= 10f
                    ? currentCooldown.ToString("0")
                    : currentCooldown.ToString("0.0");
            }
        }

        private void UpdateReadyState(bool isReady)
        {
          //  Debug.Log($" [UI] {content.Description.Title} {(isReady? "Ready" : "Not Ready")}");
            usableIconReadyGlowBorder.gameObject.SetActive(isReady);
        }

        private void SetVisible(bool isVisible)
        {
            if (usableIcon != null)
            {
                usableIcon.gameObject.SetActive(isVisible);
            }

            if (costIcon != null)
            {
                costIcon.gameObject.SetActive(isVisible);
            }

            if (costIconText != null)
            {
                costIconText.gameObject.SetActive(isVisible);
            }

            if (chargesText != null)
            {
                chargesText.gameObject.SetActive(isVisible);
            }

            if (usableIconCooldownFill != null)
            {
                usableIconCooldownFill.gameObject.SetActive(isVisible);
            }

            if (usableIconCooldownBorderFill != null)
            {
                usableIconCooldownBorderFill.gameObject.SetActive(isVisible);
            }

            if (usableIconReadyGlowBorder != null)
            {
                usableIconReadyGlowBorder.gameObject.SetActive(isVisible);
            }

            if (usableIconCooldownText != null)
            {
                usableIconCooldownText.gameObject.SetActive(isVisible);
            }
            if (hotkeyText != null)
            {
                hotkeyText.gameObject.SetActive(isVisible);
            }
        }

        /// <summary>
        /// Вызывается системой применения после попытки использовать способность.
        /// </summary>
        public void OnUse(bool success)
        {
            rectTransform.DOKill();

            if (!success)
            {
                rectTransform.DOShakeAnchorPos(
                    failedUseShakeDuration,
                    failedUseShakeStrength,
                    vibrato: 16,
                    randomness: 90f,
                    snapping: false,
                    fadeOut: true
                );

                return;
            }

            rectTransform.DOPunchScale(
                Vector3.one * usePunchStrength,
                usePunchDuration,
                vibrato: 6,
                elasticity: 0.7f
            );

            // Если данные в content изменились сразу после использования,
            // визуал обновится в этот же кадр.
            RefreshVisuals(force: true);
        }
    }
}

