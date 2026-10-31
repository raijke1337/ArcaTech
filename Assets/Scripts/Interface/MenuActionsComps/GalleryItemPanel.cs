using System;
using Arcatech.Items;
using Arcatech.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arcatech.UI
{
    /// <summary>
    /// «Панель коллекционного предмета» Мастерской: одна плашка ловушки или костюма.
    /// Сама ничего не сохраняет: показывает состояние и сообщает о нажатиях контроллеру.
    /// </summary>
    public class GalleryItemPanel : MonoBehaviour
    {
        [Header("Вид")]
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text titleText;
        [Tooltip("Знак вопроса для ненайденного чертежа")]
        [SerializeField] private Sprite hiddenSprite;

        private Color lockedTint;
        [SerializeField] private string hiddenTitle = "???";

        [Header("Разблокировка")]
        [SerializeField] private Button unlockButton;
        [Tooltip("Цена в деталях на кнопке «Разблокировать»")]
        [SerializeField] private TMP_Text unlockCostText;

        [Header("Действие (после разблокировки)")]
        [SerializeField] private Button actionButton;
        [SerializeField] private TMP_Text actionLabel;
        [SerializeField] private string exploreLabel = "Исследовать";
        [SerializeField] private string equipLabel = "Экипировать";
        [SerializeField] private string equippedLabel = "Надето";

        public GalleryEntry Entry { get; private set; }
        public bool IsCostume { get; private set; }

        public event Action<GalleryItemPanel> UnlockClicked;
        public event Action<GalleryItemPanel> ActionClicked;

        private void Awake()
        {
            lockedTint = GameInterfaceManager.Instance.ColorReference.Locked;
            ;
            if (unlockButton != null) unlockButton.onClick.AddListener(OnUnlockButton);
            if (actionButton != null) actionButton.onClick.AddListener(OnActionButton);
        }

        private void OnDestroy()
        {
            if (unlockButton != null) unlockButton.onClick.RemoveListener(OnUnlockButton);
            if (actionButton != null) actionButton.onClick.RemoveListener(OnActionButton);
        }

        private void OnUnlockButton() => UnlockClicked?.Invoke(this);
        private void OnActionButton() => ActionClicked?.Invoke(this);

        /// <summary>Привязывает панель к записи. Вызывается один раз при создании.</summary>
        public void Setup(GalleryEntry entry, bool isCostume)
        {
            Entry = entry;
            IsCostume = isCostume;
        }

        /// <param name="state">Состояние чертежа.</param>
        /// <param name="partsCount">Сколько деталей у игрока (кнопка «Разблокировать» активна, если хватает).</param>
        /// <param name="isEquipped">Костюм уже надет (кнопка «Надето» неактивна).</param>
        public void Refresh(GalleryEntryState state, int partsCount, bool isEquipped)
        {
            if (Entry == null) return;

            var description = Entry.ResolveDescription();
            var picture = description != null ? description.Picture : null;

            switch (state)
            {
                case GalleryEntryState.Hidden:
                    SetIcon(hiddenSprite, lockedTint);
                    SetTitle(hiddenTitle);
                    SetVisible(unlockButton, false);
                    SetVisible(actionButton, false);
                    break;

                case GalleryEntryState.Found:
                    SetIcon(picture, lockedTint);
                    SetTitle(Entry.ResolveTitle());
                    SetVisible(unlockButton, true);
                    if (unlockButton != null) unlockButton.interactable = partsCount >= Entry.unlockCost;
                    if (unlockCostText != null) unlockCostText.text = Entry.unlockCost.ToString();
                    SetVisible(actionButton, false);
                    break;

                default: // Unlocked
                    SetIcon(picture, Color.white);
                    SetTitle(Entry.ResolveTitle());
                    SetVisible(unlockButton, false);
                    SetVisible(actionButton, true);
                    if (actionButton != null) actionButton.interactable = !(IsCostume && isEquipped);
                    if (actionLabel != null)
                        actionLabel.text = !IsCostume ? exploreLabel : isEquipped ? equippedLabel : equipLabel;
                    break;
            }
        }

        private void SetIcon(Sprite sprite, Color tint)
        {
            if (icon == null) return;
            icon.sprite = sprite;
            icon.color = tint;
            icon.enabled = sprite != null;
        }

        private void SetTitle(string text)
        {
            if (titleText != null) titleText.text = text;
        }

        private static void SetVisible(Button button, bool visible)
        {
            if (button != null) button.gameObject.SetActive(visible);
        }
    }
}
