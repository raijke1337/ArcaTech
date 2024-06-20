using Arcatech.EventBus;
using Arcatech.Texts;
using Arcatech.UI;
using System.Collections;
using UnityEngine;
namespace Arcatech.Managers
{
    public class GameInterfaceManager : LoadedManagerBase
    {
        [SerializeField] private TargetPanel _tgtPan;
        [SerializeField] private PlayerUnitPanel _playerPan;
        [SerializeField] private GameTextComp _text;
        [SerializeField] private GameObject _ded;
        [SerializeField] private GameObject _pause;

        [Space, SerializeField] private CartoonFX.CFXR_ParticleText _textPrefab;


        EventBinding<DrawDamageEvent> _drawDamageBind;



        #region managed
        public override void Initiate()
        {


            if (GameManager.Instance.GetCurrentLevelData.LevelType == LevelType.Game)
            {
                _playerPan.IsNeeded = true;
                _playerPan.StartController();
                _tgtPan.IsNeeded = false;
                _text.gameObject.SetActive(false);
                _text.DialogueCompleteEvent += OnDialogueCompletedInTextWindow;
                _ded.SetActive(false);

                _drawDamageBind = new EventBinding<DrawDamageEvent>(DrawDamageNumber);
                EventBus<DrawDamageEvent>.Register(_drawDamageBind);

            }
            else
            {
                _playerPan.IsNeeded = false;
                _tgtPan.IsNeeded = false;
                _text.gameObject.SetActive(false);
                _text.DialogueCompleteEvent += OnDialogueCompletedInTextWindow; // dialogues also hap[pen in scene levels
                _ded.SetActive(false);
            }

        }



        public override void RunUpdate(float delta)
        {
            _playerPan.UpdateController(delta);
            if (_tgtPan.IsNeeded) _tgtPan.UpdateController(delta);
        }

        public override void Stop()
        {
            EventBus<DrawDamageEvent>.Unregister(_drawDamageBind);
            _playerPan.StopController();
        }
        #endregion


        #region game dialogues and texts


        public void UpdateGameText(DialoguePart text, bool isShown)
        {

            if (isShown)
            {
                _playerPan.LoadedDialogue(text, isShown);
                _text.gameObject.SetActive(isShown);
                _text.CurrentDialogue = text;
                if (text.Options.Count > 0)
                {
                    GameManager.Instance.OnPlayerPaused(true);
                }
            }    
            else
            {
                _playerPan.LoadedDialogue(text, isShown);
                _text.gameObject.SetActive(isShown);
                GameManager.Instance.OnPlayerPaused(false);
            }

        }
        private void OnDialogueCompletedInTextWindow()
        {
            GameManager.Instance.OnPlayerPaused(false);
            _text.gameObject.SetActive(false);
        }




        #endregion

        #region target panel
        public void OnPlayerSelectedTargetable(BaseTargetableItem item, bool show)
        {
            // comm ented because I dont like the panel
            //if (show)
            //{
            //    if (_cor != null)
            //    {
            //        StopAllCoroutines();
            //    }

            //    paneltimer = _selPanelDisappearTimer;
            //    _tgt.IsNeeded = true;
            //    _tgt.AssignItem(item);
            //}
            //if (!show)
            //{
            //    paneltimer = _selPanelDisappearTimer;
            //    _cor = StartCoroutine(HidePanel(_tgt));
            //}
        }
        private float paneltimer;
        private IEnumerator HidePanel(PanelWithBarGeneric item)
        {
            while (paneltimer > 0)
            {
                paneltimer -= Time.deltaTime;
                yield return null;
            }
            item.IsNeeded = false;
            yield return null;
        }
        #endregion


        #region draw damage
        private void DrawDamageNumber (DrawDamageEvent data)
        {
            var txt = Instantiate(_textPrefab,data.Unit.transform);
            txt.UpdateText(data.Damage.ToString());
        }

        #endregion


        #region menus

        public void OnPauseRequesShowPanelAndPause(bool isPause)
        {
            _pause.SetActive(isPause);
            GameManager.Instance.OnPlayerPaused(isPause);
        }

        public void GameOver()
        {
            GameManager.Instance.OnPlayerPaused(true);
            _ded.SetActive(true);
        }
        public void ToMain()
        {
            GameManager.Instance.OnReturnToMain();
        }
        public void OnRestart()
        {
            GameManager.Instance.RequestLoadSceneFromContainer(GameManager.Instance.GetCurrentLevelData);
        }

        #endregion

    }


}
