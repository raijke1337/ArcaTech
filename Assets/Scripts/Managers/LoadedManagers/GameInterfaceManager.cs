
using System.Collections.Generic;
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
            // if (GameManager.Instance.GetCurrentLevelData.LevelType != LevelType.Game)
            // {
            //     _playerPan.gameObject.SetActive(false);
            //     _ded.SetActive(false);
            // }
           // else
            {
                _playerPan.gameObject.SetActive(true);
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
        }

        #endregion
#region draw damage
        // ---------------------------
        // Floating Texts (Damage/Heal)
        // ---------------------------
        [Header("Floating Texts")]
        [Tooltip("Canvas to draw floating texts (Screen Space - Overlay/Camera or World Space).")]
        [SerializeField] private Canvas uiCanvas;
        [Tooltip("Optional container under the canvas for floating texts.")]
        [SerializeField] private RectTransform floatingTextsParent;
        [Tooltip("UI prefab with DamageTextUI component.")]
        [SerializeField] private DamageTextUI damageTextPrefab;
        [SerializeField, Min(1)] private int damageTextPoolSize = 16;

        [Header("Floating Texts Look & Feel")]
        [SerializeField] private float pixelOffsetMagnitude = 50f;
        [SerializeField] private float upwardTravel = 80f;
        [SerializeField] private float defaultDisplayDuration = 0.9f;
        [SerializeField] private Color damageColor = new Color(0.95f, 0.25f, 0.25f);
        [SerializeField] private Color healColor = new Color(0.25f, 0.95f, 0.25f);

        private readonly Queue<DamageTextUI> _damagePool = new Queue<DamageTextUI>();
        private Camera _mainCamera;

        private void EnsureFloatingTextSetup()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            if (uiCanvas == null)
            {
                uiCanvas = FindObjectOfType<Canvas>();
                if (uiCanvas == null)
                    Debug.LogError("GameInterfaceManager: No Canvas assigned/found for floating texts.");
            }
            if (floatingTextsParent == null && uiCanvas != null)
                floatingTextsParent = uiCanvas.transform as RectTransform;

            if (_damagePool.Count == 0 && damageTextPrefab != null)
            {
                for (int i = 0; i < damageTextPoolSize; i++)
                    CreateDamageTextInstance();
            }
        }

        private DamageTextUI CreateDamageTextInstance()
        {
            var parent = floatingTextsParent != null ? floatingTextsParent
                : (uiCanvas != null ? uiCanvas.transform as RectTransform : null);

            var inst = Instantiate(damageTextPrefab, parent);
            inst.gameObject.SetActive(false);
            _damagePool.Enqueue(inst);
            return inst;
        }

        private DamageTextUI GetDamageText()
        {
            if (_damagePool.Count > 0)
            {
                var inst = _damagePool.Dequeue();
                inst.gameObject.SetActive(true);
                return inst;
            }
            Debug.LogWarning("GameInterfaceManager: Floating text pool exhausted. Expanding pool.");
            return CreateDamageTextInstance();
        }

        private void ReturnDamageText(DamageTextUI inst)
        {
            if (inst == null) return;
            inst.StopAndReset();
            inst.gameObject.SetActive(false);
            _damagePool.Enqueue(inst);
        }

        public void ShowFloatingNumber(float amount, Vector3 worldPosition, bool isDamage, float? durationOverride = null)
        {
            if (amount <= 0f) return;

            EnsureFloatingTextSetup();
            if (uiCanvas == null || floatingTextsParent == null || damageTextPrefab == null) return;

            // World -> Screen
            Vector3 screenPos = _mainCamera != null ? _mainCamera.WorldToScreenPoint(worldPosition) : worldPosition;

            // Screen -> Local (anchored)
            Camera camForCanvas = null;
            if (uiCanvas.renderMode == RenderMode.ScreenSpaceCamera || uiCanvas.renderMode == RenderMode.WorldSpace)
                camForCanvas = uiCanvas.worldCamera;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    floatingTextsParent,
                    screenPos,
                    camForCanvas,
                    out Vector2 localPoint))
                return;

            // Random pixel offset (upward bias)
            Vector2 randomOffset = new Vector2(
                Random.Range(-pixelOffsetMagnitude, pixelOffsetMagnitude),
                Random.Range(0f, pixelOffsetMagnitude * 2f)
            );
            Vector2 startPos = localPoint + randomOffset;

            var inst = GetDamageText();
            string content = isDamage ? amount.ToString("F0") : "+" + amount.ToString("F0");
            Color color = isDamage ? damageColor : healColor;
            float duration = durationOverride.HasValue ? durationOverride.Value : defaultDisplayDuration;

            inst.Play(
                content,
                color,
                startPos,
                upwardTravel,
                duration,
                () => ReturnDamageText(inst)
            );
        }
        // ---------------------------

#endregion
        
    }


}
