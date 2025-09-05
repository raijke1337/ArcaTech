using Arcatech.EventBus;
using Arcatech.Texts;
using Arcatech.UI;
using Arcatech.Units;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Arcatech.Managers
{
    public class GameInterfaceManager : MonoBehaviour, IManagedController
    {

        public static GameInterfaceManager Instance;
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this.gameObject);
        }


        [SerializeField] private TargetPanel _tgtPan;
        [SerializeField] private PlayerUnitPanel _playerPan;
        [SerializeField] private GameTextWindowComponent _text;
        [SerializeField] private GameObject _ded;
        [SerializeField] private GameObject _pause;

        EventBinding<PlayerStatsChangedUIEvent> _statChangedBind;
        
        EventBinding<PauseToggleEvent> _pauseToggleBind;
        EventBinding<PlayerTargetUpdateEvent> _targetUpdateBinding;
        

        #region managed
        public virtual void StartController()
        {
        }


        public virtual void FixedControllerUpdate(float fixedDelta)
        {

        }

        public virtual void ControllerUpdate(float delta)
        {

        }

        private void OnEnable()
        {
            _pauseToggleBind = new EventBinding<PauseToggleEvent>(ShowPauseMenu);
            _targetUpdateBinding = new EventBinding<PlayerTargetUpdateEvent>(OnTargetUpdate);

            EventBus<PauseToggleEvent>.Register(_pauseToggleBind);
            EventBus<PlayerTargetUpdateEvent>.Register(_targetUpdateBinding);
        }

        private void Start()
        {

            if (GameManager.Instance.GetCurrentLevelData.LevelType == LevelType.Game)
            {
                _playerPan.gameObject.SetActive(true);

                _text.gameObject.SetActive(false);
                _text.DialogueCompleteEvent += OnDialogueCompletedInTextWindow;
                _ded.SetActive(false);
                _tgtPan.gameObject.SetActive(false); /// placeholder
            }
            else
            {
                _playerPan.gameObject.SetActive(false);
                _text.gameObject.SetActive(false);
                _text.DialogueCompleteEvent += OnDialogueCompletedInTextWindow; // dialogues also happen in scene levels
                _ded.SetActive(false);
                _tgtPan.gameObject.SetActive(false);
            }
        }
        private void OnDisable()
        {
            EventBus<PauseToggleEvent>.Deregister(_pauseToggleBind);
            EventBus<PlayerTargetUpdateEvent>.Deregister(_targetUpdateBinding);
        }
        public virtual void StopController()
        {
        }
        #endregion


        #region game dialogues and texts


        public void UpdateGameText(DialoguePart text, bool isShown)
        {

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
            }

        }
        private void OnDialogueCompletedInTextWindow()
        {
            EventBus<PauseToggleEvent>.Raise(new PauseToggleEvent(false));
            _text.gameObject.SetActive(false);
        }


        #endregion

 
        #region UI events from event bus


        void OnTargetUpdate(PlayerTargetUpdateEvent e)
        {
           // Debug.Log($"{e}");
            if (e.Target is PlayerUnit) return; //dont show playuer

            //_tgtPan.UpdateTargeted(e.Target);
            //_tgtPan.gameObject.SetActive(e.Target != null);


        }

        #endregion


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
