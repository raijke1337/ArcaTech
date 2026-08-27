using Arcatech.Managers;
using Arcatech.Scenes;
using TMPro;
using UnityEngine;
namespace Arcatech.UI
{
    public class LevelButtonComp : MonoBehaviour
    {
        [SerializeField] private SceneContainer _data;
        private TextMeshProUGUI _text;
       // public event SimpleEventsHandler<SceneContainer> OnButtonClick;
        private RectTransform _rect;
        public Vector2 GetSize
        {
            get
            {
                if (_rect == null) _rect = GetComponent<RectTransform>();
                return _rect.sizeDelta;
            }
        }
        public SceneContainer LevelData // set for instantiated buttons
        {
            get
            {
                return _data;
            }
            set
            {
                _text = GetComponentInChildren<TextMeshProUGUI>();
                _text.text = value.Description.Title;
                _data = value;
            }
        }
        private void OnEnable() // used for default keys
        {
            if (_data != null)
            {
                _text = GetComponentInChildren<TextMeshProUGUI>();
                _text.text = _data.Description.Title;
            }
        }
        public void ClickedEventCall()
        {
           // GameManager.Instance.RequestLoadSceneFromContainer(_data);
        }

    }
}