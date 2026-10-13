using Arcatech.Managers;
using UnityEngine;

namespace Arcatech.UI
{
    public class MenuPrefabControllerComp : MonoBehaviour
    {
        [Tooltip("Кнопка «Продолжить» - показывается, только если есть сохранение")]
        [SerializeField] private GameObject continueButton;
        [Tooltip("Окно «Подтверждение перезаписи» для «Новой игры». Пусто - без подтверждения.")]
        [SerializeField] private GameObject overwriteConfirmWindow;

        private void Start()
        {
            if (continueButton != null) continueButton.SetActive(GameManager.Instance.HasSaveToContinue);
            if (overwriteConfirmWindow != null) overwriteConfirmWindow.SetActive(false);
        }

        public void OnContinue()
        {
            GameManager.Instance.OnContinueButton();
        }

        public void OnNew()
        {
            if (overwriteConfirmWindow != null && GameManager.Instance.HasSaveToContinue)
            {
                overwriteConfirmWindow.SetActive(true);
                return;
            }
            GameManager.Instance.OnStartNewGameButton();
        }

        /// <summary>«Да» в окне подтверждения перезаписи.</summary>
        public void OnConfirmOverwrite()
        {
            if (overwriteConfirmWindow != null) overwriteConfirmWindow.SetActive(false);
            GameManager.Instance.OnStartNewGameButton();
        }

        /// <summary>«Нет» в окне подтверждения перезаписи.</summary>
        public void OnCancelOverwrite()
        {
            if (overwriteConfirmWindow != null) overwriteConfirmWindow.SetActive(false);
        }

        public void OnGallery()
        {
            GameManager.Instance.OnGalleryButton();
        }
        public void OnQuitGame()
        {
            GameManager.Instance.OnExitButton();
        }

    }
}
