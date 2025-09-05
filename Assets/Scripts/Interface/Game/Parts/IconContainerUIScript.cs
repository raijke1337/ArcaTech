using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arcatech.UI
{
    public class IconContainerUIScript : MonoBehaviour
    { 
        [SerializeField] private Image _timerFill; // 0 means item is ready
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _text;
        IIconContent iconContent;

        public void AssignIcon(IIconContent content) => iconContent = content;
        public void OnUse()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.DOShakePosition(0.5f,3);
        }

        private void Update()
        {
            if (iconContent != null)
            {

                _icon.sprite = iconContent.Icon;
                _text.text = iconContent.IconValue;
                _timerFill.fillAmount = iconContent.FillValue;
            }
        }

    }
}