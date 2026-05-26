using System.Collections;
using System.Collections.Generic;
using Arcatech.Interactions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Arcatech.MiniGames
{
    public class TimedButtonsPressGameComponent : MiniGameBase
{
    [System.Serializable]
    public class ButtonIconConfig
    {
        public KeyCode key;
        public Sprite icon;
        public Color keyColor = Color.white;
    }

    private class ButtonIcon
    {
        public GameObject gameObject;
        public Image iconImage;
        public RectTransform rectTransform;
        public KeyCode keyCode;
        public float windowDuration;
        public RectTransform trackLine;
        public RectTransform targetCircle;
        public float travelTime;
        public bool processed = false;
        public bool passedTarget = false;
        private Tween moveTween;

        public ButtonIcon(GameObject go)
        {
            gameObject = go;
            iconImage = go.GetComponent<Image>();
            rectTransform = go.GetComponent<RectTransform>();
        }

        public void Initialize(ButtonIconConfig config, RectTransform track, float duration, RectTransform target)
        {
            keyCode = config.key;
            trackLine = track;
            targetCircle = target;
            windowDuration = duration;
            processed = false;
            passedTarget = false;
            travelTime = duration;

            // Устанавливаем иконку
            if (iconImage != null && config.icon != null)
                iconImage.sprite = config.icon;

            if (iconImage != null)
                iconImage.color = config.keyColor;

            // ВАЖНО: проверяем размер track
            float trackWidth = trackLine.rect.width;

            // Позиционируем в начале трека (слева)
            rectTransform.anchoredPosition = new Vector2(-trackLine.rect.width / 2, 0);
    

            // Анимируем движение по треку (слева направо)
            moveTween = rectTransform.DOAnchorPos(new Vector2(trackLine.rect.width / 2, 0), travelTime)
                .SetEase(Ease.Linear);

            // Масштабируем при появлении
            rectTransform.localScale = Vector3.zero;
            rectTransform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
        }

        public bool IsInWindow()
        {
            // Проверяем, находится ли иконка в окне целевого круга
            if (targetCircle == null || trackLine == null)
                return false;

            float targetX = targetCircle.anchoredPosition.x;
            float iconX = rectTransform.anchoredPosition.x;
            float windowWidth = 60f; // Ширина окна для нажатия

            return Mathf.Abs(iconX - targetX) < windowWidth;
        }

        public bool HasPassedTarget()
        {
            if (targetCircle == null || trackLine == null || passedTarget)
                return false;

            float targetX = targetCircle.anchoredPosition.x;
            float iconX = rectTransform.anchoredPosition.x;

            // Если иконка прошла точку цели
            if (iconX > targetX)
            {
                passedTarget = true;
                return true;
            }

            return false;
        }

        public void Destroy()
        {
            if (moveTween != null)
                moveTween.Kill();

            if (gameObject != null)
                Object.Destroy(gameObject);
        }

        public void SetActive(bool active)
        {
            if (gameObject != null)
                gameObject.SetActive(active);
        }

        public void PlayDisappearAnimation(System.Action onComplete = null)
        {
            if (moveTween != null)
                moveTween.Kill();

            rectTransform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack)
                .OnComplete(() => 
                {
                    SetActive(false);
                    onComplete?.Invoke();
                });
        }
    }

    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 30f;
    [SerializeField] private float iconSpawnRate = 1.5f;
    [SerializeField] private float windowDuration = 3f;
    [SerializeField] private float progressIncrement = 0.1f;
    [SerializeField] private float progressDecrement = 0.15f;
    [SerializeField] private float missedWindowDecrement = 0.1f;

    [Header("UI References")]
    [SerializeField] private RectTransform trackLine;
    [SerializeField] private Image progressBar;
    [SerializeField] private RectTransform targetCircle;
    [SerializeField] private Transform iconPoolParent;
    [SerializeField] private Button cancelButton;

    [Header("Prefabs")]
    [SerializeField] private GameObject buttonIconPrefab;

    [Header("Button Configurations")]
    [SerializeField] private ButtonIconConfig[] buttonConfigs = new ButtonIconConfig[]
    {
        new ButtonIconConfig { key = KeyCode.W },
        new ButtonIconConfig { key = KeyCode.A },
        new ButtonIconConfig { key = KeyCode.S },
        new ButtonIconConfig { key = KeyCode.D }
    };

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem successParticles;
    [SerializeField] private ParticleSystem failureParticles;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip failureSound;
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color failureColor = Color.red;



    private float currentProgress = 0.5f;
    private bool gameActive = false;
    private float gameTimer = 0f;
    private Queue<ButtonIcon> iconPool = new Queue<ButtonIcon>();
    private List<ButtonIcon> activeIcons = new List<ButtonIcon>();
    private AudioSource audioSource;
    private CanvasGroup canvasGroup;
    private Color originalProgressBarColor;

    private void OnEnable()
    {
        if (cancelButton != null)
            cancelButton.onClick.AddListener(CancelGame);
    }

    private void OnDisable()
    {
        if (cancelButton != null)
            cancelButton.onClick.RemoveListener(CancelGame);
        
        StopAllCoroutines();
        DOTween.Kill(this);
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        InitializeIconPool();
        InitializeUI();
    }

    private void OnValidate()
    {
        // Проверяем, что все referencias инициализированы
        if (trackLine == null)
        {
            Debug.LogError("Track Line не назначен в инспекторе!");
            return;
        }

        if (buttonIconPrefab == null)
        {
            Debug.LogError("Button Icon Prefab не назначен в инспекторе!");
            return;
        }
    }

    private void InitializeIconPool()
    {
        // Создаем пул иконок для переиспользования
        for (int i = 0; i < 20; i++)
        {
            GameObject iconObj = Instantiate(buttonIconPrefab, iconPoolParent);
            ButtonIcon buttonIcon = new ButtonIcon(iconObj);
            iconObj.SetActive(false);
            iconPool.Enqueue(buttonIcon);
        }
    }

    private void InitializeUI()
    {
        currentProgress = 0.1f;
        progressBar.fillAmount = currentProgress;
        originalProgressBarColor = progressBar.color;
        UpdateProgressBarColor();
    }


    public override void StartGame()
    {
        gameActive = true;
        gameTimer = 0f;
        currentProgress = 0.1f;
        activeIcons.Clear();
        UpdateProgressBarColor();

        // Форсируем пересчёт Layout'а
        LayoutRebuilder.ForceRebuildLayoutImmediate(trackLine);
    
        // Небольшая задержка перед началом спавна
        StartCoroutine(GameLoopWithDelay());
    }

    public override void EndGame()
    {
        CancelGame();
    }

    private IEnumerator GameLoopWithDelay()
    {
        // Ждем один кадр, чтобы Layout полностью инициализировался
        yield return new WaitForEndOfFrame();
    
        StartCoroutine(GameLoop());
        StartCoroutine(SpawnIconsRoutine());
        StartCoroutine(CheckMissedIconsRoutine());
    }
    
    private void Update()
    {
        if (!gameActive)
            return;

        gameTimer += Time.deltaTime;

        // Проверяем нажатия клавиш
        foreach (ButtonIconConfig config in buttonConfigs)
        {
            if (Input.GetKeyDown(config.key))
            {
                HandleKeyPress(config.key);
            }
        }
    }

    private void HandleKeyPress(KeyCode pressedKey)
    {
        // Находим первую активную иконку в окне, которая ещё не обработана
        ButtonIcon targetIcon = null;
        
        for (int i = 0; i < activeIcons.Count; i++)
        {
            if (activeIcons[i].IsInWindow() && activeIcons[i].keyCode == pressedKey && !activeIcons[i].processed)
            {
                targetIcon = activeIcons[i];
                break;
            }
        }

        if (targetIcon != null)
        {
            targetIcon.processed = true;
            OnSuccessfulPress(targetIcon);
        }
        else
        {
            // Если нажали, но не было активной иконки - неудача
            OnFailedPress();
        }
    }

    private void OnSuccessfulPress(ButtonIcon icon)
    {
        float targetX = icon.targetCircle.anchoredPosition.x;
        float iconX = icon.rectTransform.anchoredPosition.x;
        float distance = Mathf.Abs(iconX - targetX);

// Максимальное расстояние для окна (половина ширины окна)
        float maxDistance = 60f / 2f; // 30f, исходя из windowWidth = 60f в IsInWindow()

// Прогрессный множитель: 100% в центре, 10% на краю
        float multiplier = Mathf.Clamp(1f - (distance / maxDistance) * 0.9f, 0.1f, 1f);

// Вычисляем инкремент прогресса на основе точности
        float actualIncrement = multiplier * progressIncrement;
        currentProgress = Mathf.Min(1f, currentProgress + actualIncrement);
        
        progressBar.DOFillAmount(currentProgress, 0.3f).SetEase(Ease.OutQuad);

        // Визуальный эффект успеха
        PlaySuccessEffect(icon.rectTransform.position);
        
        if (audioSource != null && successSound != null)
            audioSource.PlayOneShot(successSound);

        UpdateProgressBarColor();

        // Удаляем иконку
        icon.PlayDisappearAnimation(() =>
        {
            activeIcons.Remove(icon);
            ReturnIconToPool(icon);
        });

        CheckGameCompletion();
    }

    private void OnFailedPress()
    {
        currentProgress = Mathf.Max(0f, currentProgress - progressDecrement);
        progressBar.DOFillAmount(currentProgress, 0.3f).SetEase(Ease.OutQuad);

        // Визуальный эффект неудачи
        PlayFailureEffect();
        
        if (audioSource != null && failureSound != null)
            audioSource.PlayOneShot(failureSound);

        UpdateProgressBarColor();
        CheckGameCompletion();
    }

    private void OnMissedIcon()
    {
        currentProgress = Mathf.Max(0f, currentProgress - missedWindowDecrement);
        progressBar.DOFillAmount(currentProgress, 0.3f).SetEase(Ease.OutQuad);

        // Визуальный эффект неудачи (более мягкий)
        PlayMissEffect();
        
        if (audioSource != null && failureSound != null)
            audioSource.PlayOneShot(failureSound);

        UpdateProgressBarColor();
        CheckGameCompletion();
    }

    private void PlaySuccessEffect(Vector3 position)
    {
        if (successParticles != null)
        {
            successParticles.transform.position = position;
            successParticles.Play();
        }

        // Пульсация шкалы прогресса
        progressBar.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 1, 1f).SetEase(Ease.OutQuad);

        // Подсветка целевого круга
        Image circleImage = targetCircle.GetComponent<Image>();
        if (circleImage != null)
        {
            Color originalColor = circleImage.color;
            circleImage.DOColor(successColor, 0.1f)
                .OnComplete(() => circleImage.DOColor(originalColor, 0.2f));
        }
    }

    private void PlayFailureEffect()
    {
        if (failureParticles != null)
        {
            failureParticles.transform.position = targetCircle.position;
            failureParticles.Play();
        }

        // Шейк шкалы прогресса
        progressBar.transform.DOShakePosition(0.4f, strength: 10f, vibrato: 10, randomness: 0.5f, snapping: false);

        // Подсветка целевого круга красным
        Image circleImage = targetCircle.GetComponent<Image>();
        if (circleImage != null)
        {
            Color originalColor = circleImage.color;
            circleImage.DOColor(failureColor, 0.1f)
                .OnComplete(() => circleImage.DOColor(originalColor, 0.2f));
        }

        // Вращение целевого круга
        targetCircle.DOLocalRotate(Vector3.forward * -15f, 0.1f)
            .OnComplete(() => targetCircle.DOLocalRotate(Vector3.zero, 0.1f));
    }

    private void PlayMissEffect()
    {
        // Более мягкий эффект для пропущенной иконки
        progressBar.transform.DOShakePosition(0.2f, strength: 5f, vibrato: 5, randomness: 0.3f, snapping: false);

        Image circleImage = targetCircle.GetComponent<Image>();
        if (circleImage != null)
        {
            Color originalColor = circleImage.color;
            circleImage.DOColor(failureColor, 0.08f)
                .OnComplete(() => circleImage.DOColor(originalColor, 0.15f));
        }
    }

    private void UpdateProgressBarColor()
    {
        Color targetColor = currentProgress < 0.3f ? failureColor :
                           currentProgress > 0.7f ? successColor :
                           Color.white;

        progressBar.DOColor(targetColor, 0.3f);
    }

    private IEnumerator GameLoop()
    {
        while (gameActive && gameTimer < gameDuration)
        {
            yield return null;
        }

        if (gameActive && gameTimer >= gameDuration)
        {
            // Время истекло, проверяем результат
            CheckGameCompletion();
        }
    }

    private IEnumerator SpawnIconsRoutine()
    {
        while (gameActive)
        {
            SpawnRandomIcon();
            yield return new WaitForSeconds(iconSpawnRate);
        }
    }

    private IEnumerator CheckMissedIconsRoutine()
    {
        while (gameActive)
        {
            yield return new WaitForSeconds(0.1f);

            // Проверяем иконки, которые прошли мимо цели
            for (int i = activeIcons.Count - 1; i >= 0; i--)
            {
                if (activeIcons[i].HasPassedTarget() && !activeIcons[i].processed)
                {
                    // Иконка прошла, но не была нажата
                    OnMissedIcon();
                    
                    activeIcons[i].PlayDisappearAnimation(() =>
                    {
                        if (activeIcons.Count > i)
                        {
                            ReturnIconToPool(activeIcons[i]);
                            activeIcons.RemoveAt(i);
                        }
                    });
                }
            }
        }
    }

    private void SpawnRandomIcon()
    {
        if (!gameActive)
            return;

        ButtonIcon icon = GetIconFromPool();
        if (icon == null)
            return;

        ButtonIconConfig config = buttonConfigs[Random.Range(0, buttonConfigs.Length)];
        icon.Initialize(config, trackLine, windowDuration, targetCircle);
        icon.gameObject.transform.SetParent(trackLine, false);
        activeIcons.Add(icon);
    }

    private ButtonIcon GetIconFromPool()
    {
        if (iconPool.Count > 0)
        {
            ButtonIcon icon = iconPool.Dequeue();
            icon.SetActive(true);
            return icon;
        }

        // Если пула не хватает, создаем новую
        GameObject iconObj = Instantiate(buttonIconPrefab, trackLine);
        return new ButtonIcon(iconObj);
    }

    private void ReturnIconToPool(ButtonIcon icon)
    {
        icon.SetActive(false);
        icon.gameObject.transform.SetParent(iconPoolParent);
        icon.gameObject.transform.localPosition = Vector3.zero;
        iconPool.Enqueue(icon);
    }

    private void CheckGameCompletion()
    {
        if (currentProgress >= 1f)
        {
            CompleteGame(true);
        }
        else if (currentProgress <= 0f)
        {
            CompleteGame(false);
        }
    }

    private void CompleteGame(bool result)
    {
        if (!gameActive)
            return;

        gameActive = false;
        StopAllCoroutines();
        InteractionState finish;
        if (result)
        {
            finish = InteractionState.Success;
        }
        else
        {
            finish = InteractionState.Failure;
        }
        // Финальная анимация
        if (result)
        {
            PlayGameWinEffect();
        }
        else
        {
            PlayGameLoseEffect();
        }

        DOVirtual.DelayedCall(0.5f, () =>
        {
            ReportResult(finish);
            DisableGame();
        });
    }

    private void PlayGameWinEffect()
    {
        // Масштабирование прогресс бара
        progressBar.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack);

        // Вращение всего экрана/окна
        canvasGroup.transform.DOShakeRotation(0.4f, strength: 5f, vibrato: 10);

        // Множественные частицы
        if (successParticles != null)
        {
            for (int i = 0; i < 3; i++)
            {
                DOVirtual.DelayedCall(i * 0.1f, () => successParticles.Play());
            }
        }

        // Вспышка цвета
        Image bgImage = GetComponent<Image>();
        if (bgImage != null)
        {
            Color originalColor = bgImage.color;
            bgImage.DOColor(successColor, 0.1f)
                .OnComplete(() => bgImage.DOColor(originalColor, 0.3f).SetEase(Ease.OutQuad));
        }
    }

    private void PlayGameLoseEffect()
    {
        // Пульсирующий шейк
        progressBar.transform.DOShakePosition(0.5f, strength: 15f, vibrato: 15, randomness: 0.7f);

        // Мигание цвета
        Image bgImage = GetComponent<Image>();
        if (bgImage != null)
        {
            for (int i = 0; i < 3; i++)
            {
                bgImage.DOColor(failureColor, 0.1f)
                    .OnComplete(() => bgImage.DOColor(Color.white, 0.1f));
            }
        }

        // Множественные частицы взрыва
        if (failureParticles != null)
        {
            for (int i = 0; i < 2; i++)
            {
                DOVirtual.DelayedCall(i * 0.15f, () => failureParticles.Play());
            }
        }
    }

    public void CancelGame()
    {
        if (!gameActive)
            return;

        gameActive = false;
        StopAllCoroutines();

        // Плавное исчезновение
        canvasGroup.DOFade(0.5f, 0.3f);
        canvasGroup.transform.DOScale(0.8f, 0.3f).SetEase(Ease.InQuad);

        DOVirtual.DelayedCall(0.3f, () =>
        {
            ReportResult(InteractionState.Cancelled);
            DisableGame();
        });
    }

    private void DisableGame()
    {
        // Очищаем активные иконки
        foreach (ButtonIcon icon in activeIcons)
        {
            ReturnIconToPool(icon);
        }
        activeIcons.Clear();
        Debug.Log($"{this} disabling");
        gameObject.SetActive(false);
    }
}
}