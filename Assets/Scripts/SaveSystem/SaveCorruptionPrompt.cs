using System.Linq;
using TMPro;
using UnityEngine;

namespace Arcatech.SaveSystem
{
    /// <summary>Окно «Сохранение повреждено» для главного меню. Показывается, если при
    /// загрузке не сошёлся хеш. Кнопки: «Восстановить» (из *.bak) и «Начать заново».
    /// Повреждённые файлы обрабатываются по одному.</summary>
    public class SaveCorruptionPrompt : MonoBehaviour
    {
        [SerializeField] private GameObject window;
        [SerializeField] private TextMeshProUGUI messageText;
        [Tooltip("Кнопка «Восстановить» - скрывается, если копию восстановить не удалось")]
        [SerializeField] private GameObject restoreButton;

        [TextArea, SerializeField] private string corruptedMessage =
            "Файл сохранения «{0}» повреждён или изменён вручную. Восстановить предыдущее сохранение из резервной копии?";
        [TextArea, SerializeField] private string backupFailedMessage =
            "Резервная копия «{0}» тоже недоступна. Эти данные придётся начать заново.";

        private SaveSlot _current;

        private void Start() => ShowNextOrClose();

        public void OnRestoreClicked()
        {
            if (SaveManager.Instance.TryRestoreFromBackup(_current))
            {
                ShowNextOrClose();
                return;
            }

            if (messageText != null) messageText.text = string.Format(backupFailedMessage, SlotTitle(_current));
            if (restoreButton != null) restoreButton.SetActive(false);
        }

        public void OnResetClicked()
        {
            SaveManager.Instance.ResetCorrupted(_current);
            ShowNextOrClose();
        }

        private void ShowNextOrClose()
        {
            var corrupted = SaveManager.Instance.CorruptedSlots;
            if (corrupted.Count == 0)
            {
                if (window != null) window.SetActive(false);
                return;
            }

            _current = corrupted.First();
            if (window != null) window.SetActive(true);
            if (restoreButton != null) restoreButton.SetActive(true);
            if (messageText != null) messageText.text = string.Format(corruptedMessage, SlotTitle(_current));
        }

        private static string SlotTitle(SaveSlot slot) => slot switch
        {
            SaveSlot.Settings => "Настройки",
            SaveSlot.MetaProgress => "Прогресс игры",
            _ => "Чекпоинт уровня"
        };
    }
}
