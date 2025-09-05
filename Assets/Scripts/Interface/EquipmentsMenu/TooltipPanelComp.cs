using Arcatech.Texts;
using com.cyborgAssets.inspectorButtonPro;

//using com.cyborgAssets.inspectorButtonPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arcatech.UI
{
    public class TooltipPanelComp : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private TextMeshProUGUI _desc;
        [SerializeField] private TextMeshProUGUI _flavor;
        [SerializeField] private Image _icon;

        [Space, SerializeField] private ExtendedText _debugDesc;
        [ProButton]
        private void LoadDebugItem()
        {
            Texts = _debugDesc;
        }



        private ExtendedText _info;
        public ExtendedText Texts
            {
            get
            {
                return _info;

            }
            set
            {
                _info = value;
                _name.text = value.Title;
                _desc.text = value.Text;
                _flavor.text = value.FlavorText;
                _icon.sprite = value.Picture;
            }
            }



    }
}