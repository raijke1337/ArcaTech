using System;
using Arcatech.EventBus;
using Arcatech.Interactions;
using Arcatech.Texts;
using Arcatech.UI;
using Arcatech.Units;
using Arcatech.Units.Control;
using DG.Tweening;
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
        [SerializeField,Child] private FloatingTooltipComponent floatingTooltip;
        
        private Sequence tooltipSeq;
        
        
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
            if (!show)
            {
               FadeOut(floatingTooltip.transform);
                return;
            }
            floatingTooltip.Set(targetable);
            FadeIn(floatingTooltip.transform);
        }

        #region effects

        private void FadeIn(Transform window)
        {
            var cg = window.GetComponent<CanvasGroup>();

            // Cancel any in-flight tweens (prevents stale OnComplete from hiding it)
            tooltipSeq?.Kill(false);
            cg.DOKill(false);
            window.DOKill(false);

            // Make sure it's active and start from hidden values if necessary
            window.gameObject.SetActive(true);
            if (cg.alpha < 1f) cg.alpha = 0f;
            if (window.localScale.x < 1f || window.localScale.y < 1f) window.localScale = Vector3.zero;

            tooltipSeq = DOTween.Sequence()
                .Join(cg.DOFade(1f, 0.25f).SetEase(Ease.OutQuad))
                .Join(window.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
        }

        private void FadeOut(Transform window)
        {
            var cg = window.GetComponent<CanvasGroup>();

            // Cancel any in-flight tweens
            tooltipSeq?.Kill(false);
            cg.DOKill(false);
            window.DOKill(false);

            tooltipSeq = DOTween.Sequence()
                .Join(cg.DOFade(0f, 0.2f).SetEase(Ease.InQuad))
                .Join(window.DOScale(0f, 0.2f).SetEase(Ease.InBack))
                .OnComplete(() => window.gameObject.SetActive(false));

        }
        
        #endregion

        private void OnDisable()
        {
            tooltipSeq?.Kill(false);
            if (floatingTooltip != null)
            {
                var t = floatingTooltip.transform;
                t.DOKill(false);
                t.GetComponent<CanvasGroup>()?.DOKill(false);
            }
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
