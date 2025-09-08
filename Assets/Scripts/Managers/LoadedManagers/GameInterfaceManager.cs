using Arcatech.EventBus;
using Arcatech.Texts;
using Arcatech.UI;
using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Arcatech.Managers
{
    public class GameInterfaceManager : ValidatedMonoBehaviour
    {

        public static GameInterfaceManager Instance;
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this.gameObject);
        }

//unused
        //[SerializeField,Self] private TargetPanel _tgtPan;
        [SerializeField,Child] private PlayerUnitPanel _playerPan;
     //unused   //[SerializeField,Child] private GameTextWindowComponent _text;
        [SerializeField] private GameObject _ded;
        [SerializeField] private GameObject _pause;
        [SerializeField,Child,Self] private ItemCardComponent _inspectItemCard;
        

        EventBinding<PauseToggleEvent> _pauseToggleBind;
        EventBinding<BaseEntityMouseOverEvent> _mouseOverBind;
        

        #region managed

        private void OnEnable()
        {
            _pauseToggleBind = new EventBinding<PauseToggleEvent>(ShowPauseMenu);
            _mouseOverBind = new EventBinding<BaseEntityMouseOverEvent>(OnMouseOver);
            EventBus<PauseToggleEvent>.Register(_pauseToggleBind);
            EventBus<BaseEntityMouseOverEvent>.Register(_mouseOverBind);
        }

        private void Start()
        {

            if (GameManager.Instance.GetCurrentLevelData.LevelType == LevelType.Game)
            {
                _playerPan.gameObject.SetActive(true);
                _ded.SetActive(false);
            }
            else
            {
                _playerPan.gameObject.SetActive(false);
                _ded.SetActive(false);
            }
        }
        private void OnDisable()
        {
            EventBus<PauseToggleEvent>.Deregister(_pauseToggleBind);
            EventBus<BaseEntityMouseOverEvent>.Deregister(_mouseOverBind);
        }

        #endregion


        #region game dialogues and texts

        void SetupGameTextWindow()
        {
            /*_text.gameObject.SetActive(false);
            _text.DialogueCompleteEvent += OnDialogueCompletedInTextWindow;*/
        }

        public void UpdateGameText(DialoguePart text, bool isShown)
        {

            /*
            if (isShown)
            {
                //_playerPan.LoadedDialogue(text, isShown);
                _text.gameObject.SetActive(isShown);
                _text.CurrentDialogue = text;
                if (text.Options.Count > 0)
                {
                    EventBus<PauseToggleEvent>.Raise(new PauseToggleEvent(isShown));
                }
            }    
            else
            {
                //_playerPan.LoadedDialogue(text, isShown);
                _text.gameObject.SetActive(isShown);
                EventBus<PauseToggleEvent>.Raise(new PauseToggleEvent(isShown));
            }*/

        }
        private void OnDialogueCompletedInTextWindow()
        {
            EventBus<PauseToggleEvent>.Raise(new PauseToggleEvent(false));
           // _text.gameObject.SetActive(false);
        }


        #endregion

        void OnMouseOver(BaseEntityMouseOverEvent info)
        {
            // TODO: show hotkeys tooltip: use/inspect
        }




        #region menus

        
        
        public void ShowPauseMenu(PauseToggleEvent isPause)
        {
            // dont pause the game here
            _pause.SetActive(isPause.Value);
        }

        public void GameOver()
        {
            _ded.SetActive(true);
        }
        public void ToMain()
        {
            GameManager.Instance.OnReturnToMain();
        }
        public void OnRestart()
        {
            int ndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            SceneManager.LoadScene(ndex);

            //GameManager.Instance.RequestLoadSceneFromContainer(GameManager.Instance.GetCurrentLevelData);
        }
        public void ClickResume()
        {
            EventBus<PauseToggleEvent>.Raise(new PauseToggleEvent(false));
        }

        #endregion

    }


}
