using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arcatech.UI
{
    public class IconContainerUIScript : MonoBehaviour
    { 
        
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Image _timerFill; // 0 means item is ready
        [SerializeField] private Image _icon;
        IIconContent iconContent;
        private bool _isAction = false;

        public void AssignIcon(IIconContent content)
        {
            iconContent = content;
            _icon.sprite = iconContent.Description.Picture;
            _isAction = false;
            if (content is IActionIconContent action)
            {
                _text.text = action.StringInfo;
                _timerFill.fillAmount = action.FillValue;
                _isAction = true;
            }
        }
        
        public void OnUse(bool success)
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (!success) rectTransform.DOShakePosition(0.5f,3);
            else  rectTransform.DOPunchScale(Vector3.one * 0.2f,0.2f);
        }

        private void Update()
        {
            if (!_isAction) return;
            var a = iconContent as IActionIconContent; // might be slow
            _text.text = a.StringInfo;
            _timerFill.fillAmount = a.FillValue;
        }
    }
    
}