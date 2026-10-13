using Arcatech.Managers;
using Arcatech.SaveSystem;
using KBCore.Refs;
using SpankyBoy.JuiceUI.Free;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Arcatech.UI
{
    /// <summary>
    /// Интерфейс «Сводка прохождения уровня» (см. «Дизайн интерфейса»).
    /// Содержит заголовок, интерфейс «Уровень» (название, рейтинг, враги, предметы, секреты)
    /// и кнопки «Продолжить» (в «Выбор уровня») и «Перерыв» (в главное меню).
    /// Показывается из GameInterfaceManager.OnLevelCompleted.
    /// </summary>
    [RequireComponent(typeof(PanelAnimator))]
    public class LevelResultsWindow : ValidatedMonoBehaviour
    {
        [SerializeField, Self] private PanelAnimator animator;

        [Header("Интерфейс «Уровень»")]
        [SerializeField] private TextMeshProUGUI levelNameText;
        [SerializeField] private TextMeshProUGUI gradeText;
        [SerializeField] private TextMeshProUGUI killsText;
        [SerializeField] private TextMeshProUGUI itemsText;
        [Tooltip("Строка секретов. Скрывается, если на уровне нет секретов")]
        [SerializeField] private TextMeshProUGUI secretsText;
        [Tooltip("Необязательно: оставшиеся жизни")]
        [SerializeField] private TextMeshProUGUI livesText;
        [Tooltip("Необязательно: плашка «Новый рекорд»")]
        [SerializeField] private GameObject newBestMarker;

        [Header("Кнопки")]
        [SerializeField] private Button continueButton;
        [SerializeField] private Button breakButton;

        [Header("Цвета рейтинга")]
        [SerializeField] private Color gradeS = new Color32(0xFF, 0xB8, 0x4A, 0xFF); // янтарный кристалл
        [SerializeField] private Color gradeA = new Color32(0x29, 0xD7, 0xFF, 0xFF); // циан ARCA
        [SerializeField] private Color gradeB = new Color32(0x5E, 0xE6, 0xA8, 0xFF); // зелёный подтверждения
        [SerializeField] private Color gradeC = new Color32(0x91, 0xA0, 0xB8, 0xFF); // вторичный текст

        private bool _leaving;

        private void Awake()
        {
            if (continueButton != null) continueButton.onClick.AddListener(OnContinue);
            if (breakButton != null) breakButton.onClick.AddListener(OnBreak);
        }

        private void OnDestroy()
        {
            if (continueButton != null) continueButton.onClick.RemoveListener(OnContinue);
            if (breakButton != null) breakButton.onClick.RemoveListener(OnBreak);
        }

        public void Show(LevelResult result, string levelTitle)
        {
            _leaving = false;

            SetText(levelNameText, levelTitle);

            if (gradeText != null)
            {
                gradeText.text = result.grade.ToString();
                gradeText.color = GradeColor(result.grade);
            }

            SetText(killsText, $"{result.enemiesKilled}/{result.enemiesTotal}");
            SetText(itemsText, $"{result.itemsCollected}/{result.itemsTotal}");
            SetText(livesText, $"{result.livesRemaining}/{result.maxLives}");

            if (secretsText != null)
            {
                bool hasSecrets = result.secretsTotal > 0;
                secretsText.gameObject.SetActive(hasSecrets);
                if (hasSecrets) secretsText.text = $"{result.secretsFound}/{result.secretsTotal}";
            }

            if (newBestMarker != null) newBestMarker.SetActive(result.isNewBest);

            animator.Show();

            // Фокус на «Продолжить» - чтобы окно сразу управлялось с геймпада.
            if (continueButton != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
        }

        /// <summary>«Продолжить» - переход в сцену «Выбор уровня».</summary>
        public void OnContinue()
        {
            if (_leaving) return;
            _leaving = true;
            GameManager.Instance.OnLevelSelectButton();
        }

        /// <summary>«Перерыв» - переход в главное меню.</summary>
        public void OnBreak()
        {
            if (_leaving) return;
            _leaving = true;
            GameManager.Instance.OnReturnToMain();
        }

        private Color GradeColor(LevelGrade grade) => grade switch
        {
            LevelGrade.S => gradeS,
            LevelGrade.A => gradeA,
            LevelGrade.B => gradeB,
            _ => gradeC
        };

        private static void SetText(TextMeshProUGUI label, string value)
        {
            if (label != null) label.text = value;
        }
    }
}
