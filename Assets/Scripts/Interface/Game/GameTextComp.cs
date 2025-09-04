using Arcatech.Texts;
//using com.cyborgAssets.inspectorButtonPro;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Arcatech
{
    public class GameTextComp : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _speakerTitle;
        //[SerializeField] private Image _speakerPicture;


        [Space,SerializeField] private TextMeshProUGUI _mainText;
        [Space,SerializeField] private DialogueDecisionButtonComp ButtonPrefab;
        [SerializeField] private Transform ButtonPrefabParent;

 

        public event SimpleEventsHandler DialogueCompleteEvent;
        private DialoguePart _currentDialogue;

        private List<DialogueDecisionButtonComp> _buttons;



        public DialoguePart CurrentDialogue
        {
            get { return _currentDialogue; }
            set
            {
                _currentDialogue = value;
                if (value == null)
                {
                    // end dialogue
                    DialogueCompleteEvent?.Invoke();
                    return;
                }

                _speakerTitle.text = value.Character.CharacterName;
                _mainText.text = value.DialogueContent.Text;

                if (CurrentDialogue.Character.Pictures.TryGetValue(CurrentDialogue.Mood, out var p))
                {
                   // _speakerPicture.sprite = p;
                }               

                if (value.Options.Count > 0)
                {
                    foreach (var o in value.Options)
                    {
                        if (_buttons == null) _buttons = new List<DialogueDecisionButtonComp>();
                        var b = Instantiate(ButtonPrefab, ButtonPrefabParent);
                        _buttons.Add(b);
                        b.CurrentText = o.Key;
                        b.OptionClickedEvent += B_OptionClickedEvent;
                    }
                }
                else
                {

                }
            }
        }

        private void B_OptionClickedEvent(SimpleText arg)
        {
            foreach (var b in _buttons.ToArray())
            {
                b.OptionClickedEvent -= B_OptionClickedEvent;
                _buttons.Remove(b);
                Destroy(b.gameObject);
            }

            CurrentDialogue = _currentDialogue.Options[arg];
        }

        private void OnEnable()
        {
            _speakerTitle.font = GameUIManager.Instance.GetFont(FontType.Title);
            _mainText.font = GameUIManager.Instance.GetFont(FontType.Text);
        }


        [SerializeField] DialoguePart DebugDialogue;
       // [ProButton]
        public void DebugLoadDialogue()
        {
            if (DebugDialogue!=null)
            {
                CurrentDialogue = DebugDialogue;
            }
        }


    }
}