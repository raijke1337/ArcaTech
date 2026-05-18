using UnityEngine;
namespace Arcatech.Texts
{
    public class DialogueDecisionButtonComp : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI _text;
        private Description _txt;

        public event SimpleEventsHandler<Description> OptionClickedEvent;
        public Description CurrentText
        {
            get
            {
                return _txt;
            }
            set
            {
                _text.text = value.Title;
                _text.font = GameGraphicsHoster.Instance.GetFont(FontType.Text);
                _txt = value;   
            }
        }
        
        public void OnClick()
        {
            OptionClickedEvent?.Invoke(_txt);
        }

    }

}