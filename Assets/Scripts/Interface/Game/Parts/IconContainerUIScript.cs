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

        public void AssignIcon(IIconContent content)
        {
            iconContent = content;
            _icon.sprite = iconContent.Description.Picture;
            _text.text = iconContent.IconNumber;
            _timerFill.fillAmount = iconContent.FillValue;
        }
        
        public void OnUse(bool success)
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (!success) rectTransform.DOShakePosition(0.5f,3);
            else  rectTransform.DOPunchScale(Vector3.one * 0.2f,0.2f);
        }

        private void Update()
        {
            if (iconContent != null)
            {
                _icon.sprite = iconContent.Description.Picture;
                _text.text = iconContent.IconNumber;
                _timerFill.fillAmount = iconContent.FillValue;
            }
        }

    }
}