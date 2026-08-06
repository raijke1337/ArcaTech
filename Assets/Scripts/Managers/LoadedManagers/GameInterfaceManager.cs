
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
        [SerializeField,Child] private GameTextWindowComponent _text;

        [SerializeField,Child] private ItemCardComponent inspectItemCard;
        [SerializeField,Child] private FloatingTooltipComponent floatingTooltip;
        [SerializeField] public Transform miniGame;
        


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
        }
        
        #region game dialogues and texts
        public void ShowDialoguePart(DialoguePart dialogue)
        {
            if (!dialogue) return;
            _text.gameObject.SetActive(true);
            
            _text.ShowDialogue(dialogue);
        }

        public bool IsDialogueShowing => _text.gameObject.activeSelf;
        
        #endregion
        
        public void NotifyTargetable(ITargetable targetable, bool show)
        {
            if (!floatingTooltip) return;
            if (!show)
            {
                floatingTooltip.PanelAnimator.Hide();
                return;
            }
            floatingTooltip.Set(targetable);
            floatingTooltip.PanelAnimator.Show();
        }


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
