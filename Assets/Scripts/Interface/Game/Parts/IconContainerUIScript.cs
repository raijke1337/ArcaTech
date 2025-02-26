
using Arcatech.Skills;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Arcatech.UI
{
    public class IconContainerUIScript : MonoBehaviour
    { 
        [SerializeField] private Image _timerFill; // 0 means item is ready
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _text;


        public void UpdateIcon(IIconContent content)
        {

            _icon.sprite = content.Icon;
            _text.text = content.IconValue;
            _timerFill.fillAmount = content.FillValue;
        }

    }
}