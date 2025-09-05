using UnityEngine;
namespace Arcatech.Texts
{
    public class DialogueDecisionButtonComp : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI _text;
        private SimpleText _txt;

        public event SimpleEventsHandler<SimpleText> OptionClickedEvent;
        public SimpleText CurrentText
        {
            get
            {
                return _txt;
            }
            set
            {
                _text.text = value.Title;
                _text.font = GameUIManager.Instance.GetFont(FontType.Text);
                _txt = value;   
            }
        }
        
        public void OnClick()
        {
            OptionClickedEvent?.Invoke(_txt);
        }

    }

}