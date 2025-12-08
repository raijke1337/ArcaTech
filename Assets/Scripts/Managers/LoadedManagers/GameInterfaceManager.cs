using System;
using Arcatech.EventBus;
using Arcatech.Interactions;
using Arcatech.Texts;
using Arcatech.UI;
using Arcatech.Units;
using Arcatech.Units.Control;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Rendering;
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
        [SerializeField,Child] private GameTextWindowComponent _text;
        [SerializeField] private GameObject _ded;
        [SerializeField] private GameObject _pause;
        [SerializeField,Child] private ItemCardComponent inspectItemCard;
        [SerializeField,Child] private FloatingTooltipComponent aimingTooltip;
        
        

        /// <summary>
        /// called by inputs
        /// </summary>
        EventBinding<PauseToggleEvent> _pauseToggleBind;



        private void OnEnable()
        {
            _pauseToggleBind = new EventBinding<PauseToggleEvent>(ShowPauseMenu);
            EventBus<PauseToggleEvent>.Register(_pauseToggleBind);
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
            //_aim = FindFirstObjectByType<PlayerAimingComponent>();
           // if (_aim == null) Debug.LogWarning("Couldn't find PlayerAimingComponent");
        }
        
        #region game dialogues and texts
        public void HandleDialoguePart(DialoguePart dialogue, bool show)
        {
            if (!dialogue) return;
            _text.gameObject.SetActive(show);
            _text.ShowDialogue(dialogue);
        }
        #endregion
        
        public void NotifyTargetable(ITargetable targetable, bool show)
        {
            
            Debug.Log($"NotifyTargetable {targetable} {show}");
            // if (!show)
            // {
            //     aimingTooltip.gameObject.SetActive(false);
            //     return;
            // }
            // if (!aimingTooltip.gameObject.activeSelf) 
            // {
            //     aimingTooltip.gameObject.SetActive(true);
            //     aimingTooltip.Set(targetable);
            // }
        }

        #region menus

        
        
        public void ShowPauseMenu(PauseToggleEvent isPause)
        {
            // dont pause the game here
            _pause.SetActive(isPause.Value);
        }

        public void ClickResume()
        {
            EventBus<PauseToggleEvent>.Raise(new PauseToggleEvent(false));
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

        #endregion

    }


}
