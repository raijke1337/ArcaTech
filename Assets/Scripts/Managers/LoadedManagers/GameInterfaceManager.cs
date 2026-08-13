
using System;
using System.Collections.Generic;
using Arcatech.EventBus;
using Arcatech.Interactions;
using Arcatech.Stats;
using Arcatech.Texts;
using Arcatech.UI;
using AYellowpaper.SerializedCollections;
using DG.Tweening;
using KBCore.Refs;
using SpankyBoy.JuiceUI.Free;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Arcatech.Managers
{
    public class GameInterfaceManager : ValidatedMonoBehaviour
    {

        public static SerializedDictionary<ResourceStatType, Sprite> Icons;

        public static GameInterfaceManager Instance;
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this.gameObject);
        }

        [SerializeField] private PanelAnimator_Free koWindow;
        [SerializeField] private PanelAnimator_Free fade;
        [SerializeField] private PanelAnimator_Free pauseWindow;
        
        [SerializeField,Child] private PlayerUnitPanel playerPanel;

        [SerializeField,Child] private ItemCardComponent inspectItemCard;
        [SerializeField] public Transform miniGame;
        [Space]
        [SerializeField] private bool showTooltip = true;
        [SerializeField,Child] private FloatingTooltipComponent floatingTooltip;
        [Space]
        [SerializeField,Child] private GameTextWindowComponent _text;
        [SerializeField] private bool showDialogues = true;

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
            playerPanel.gameObject.SetActive(true);
            playerPanel.Show();
            koWindow.Hide();

            if (gameplayCamera == null)
                gameplayCamera = Camera.main;

            if (crosshair != null)
            {
                _crosshairRect = crosshair.GetComponent<RectTransform>();
                _crosshairCanvas = crosshair.GetComponentInParent<Canvas>();

                crosshair.CurrentTarget = null;
                crosshair.gameObject.SetActive(false);
            }
        }
        private void LateUpdate()
        {
            UpdateCrosshairPosition();
        }


        #region game dialogues and texts
        public void ShowDialoguePart(DialoguePart dialogue)
        {
            if (!dialogue || !showDialogues) return;
            _text.gameObject.SetActive(true);
            
            _text.ShowDialogue(dialogue);
        }

        public bool IsDialogueShowing => _text.gameObject.activeSelf;
        
        #endregion
        
        #region targeting
        [SerializeField] private bool showCrosshair = true;
        [SerializeField] private CrosshairComponent crosshair;
        [SerializeField] private Camera gameplayCamera;

        private RectTransform _crosshairRect;
        private Canvas _crosshairCanvas;
        public void NotifyTargetable(ITargetable targetable, bool show)
        {
            if (!showTooltip) return;
            if (!floatingTooltip) return;
            if (!show)
            {
                floatingTooltip.PanelAnimator.Hide();
                return;
            }
            floatingTooltip.gameObject.SetActive(true);
            floatingTooltip.Set(targetable);
            floatingTooltip.PanelAnimator.Show();
        }
        public void LockOnTarget(BaseGameEntityComponent target)
        {
            if (crosshair == null)
                return;

            crosshair.CurrentTarget = target;

            bool shouldShow = showCrosshair && target != null;

            crosshair.gameObject.SetActive(shouldShow);

            Debug.Log(
                target != null
                    ? $"[GameInterfaceManager] Crosshair enabled: {target.name}"
                    : "[GameInterfaceManager] Crosshair disabled");
        }
        
        private void UpdateCrosshairPosition()
        {
            if (!showCrosshair ||
                crosshair == null ||
                !crosshair.gameObject.activeSelf)
            {
                return;
            }

            BaseGameEntityComponent target = crosshair.CurrentTarget;

            if (target == null ||
                !target.gameObject.activeInHierarchy ||
                target.EffectSpawn == null)
            {
                LockOnTarget(null);
                return;
            }

            if (gameplayCamera == null)
                gameplayCamera = Camera.main;

            if (gameplayCamera == null)
                return;

            if (_crosshairRect == null)
                _crosshairRect = crosshair.GetComponent<RectTransform>();

            if (_crosshairCanvas == null)
                _crosshairCanvas = crosshair.GetComponentInParent<Canvas>();

            if (_crosshairRect == null || _crosshairCanvas == null)
                return;

            Vector3 screenPosition = gameplayCamera.WorldToScreenPoint(
                target.EffectSpawn.position);

            // Цель за камерой — прицел не показываем.
            if (screenPosition.z <= 0f)
            {
                crosshair.gameObject.SetActive(false);
                return;
            }

            RectTransform parentRect = _crosshairRect.parent as RectTransform;

            if (parentRect == null)
                return;

            Camera canvasCamera = null;

            if (_crosshairCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                canvasCamera = _crosshairCanvas.worldCamera != null
                    ? _crosshairCanvas.worldCamera
                    : gameplayCamera;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentRect,
                    screenPosition,
                    canvasCamera,
                    out Vector2 localPosition))
            {
                return;
            }

            _crosshairRect.anchoredPosition = localPosition;

            // Если цель снова оказалась перед камерой — показываем прицел.
            if (!crosshair.gameObject.activeSelf)
                crosshair.gameObject.SetActive(true);
        }
        #endregion

        #region menus

        void ShowPauseMenu(PauseToggleEvent isPause)
        {
            // dont pause the game here
            if (isPause.Value)
            {
                pauseWindow.gameObject.SetActive(true);
                pauseWindow.Show();
                fade.gameObject.SetActive(true);
                fade.Show();
            }
            else
            {
                pauseWindow.Hide();
                fade.Hide();
            }
        }

        public void ClickResume()
        {
            EventBus<PauseToggleEvent>.Raise(new PauseToggleEvent(false));
        }

        
        public void ShowPlayerDeadMenu()
        {
            koWindow.gameObject.SetActive(true);
            fade.gameObject.SetActive(true);
            koWindow.Show();
            fade.Show();
        }
        public void ToMain()
        {
            GameManager.Instance.OnReturnToMain();
        }
        public void OnRestart()
        {
            var currentLevelID = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentLevelID);
        }
        public void OnRestartAtCheckpoint()
        {
            var currentLevelID = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentLevelID);
        }

        #endregion
        
        #region glitch effect

        public void ShowGlitchEffect()
        {
            // TODO!
            // interface shake effect
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
