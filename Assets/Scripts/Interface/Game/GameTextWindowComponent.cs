using System.Collections;
using System.Collections.Generic;
using Arcatech.Texts;
using KBCore.Refs;
using TMPro;
using UnityEngine;

public class GameTextWindowComponent : ValidatedMonoBehaviour
{
    [Space, Header("Text settings")]
    [SerializeField] private TextMeshProUGUI _mainText;
    [SerializeField] private float letterDelay = 0.1f;
    [SerializeField] private float fullTextDuration = 1.276f;
    [SerializeField, Self] private RectTransform rect;
    [SerializeField] private float textPadding = 10f;

    [Header("Speaker Name Formatting")]
    [SerializeField] private string nameFormat = "<b>{0}:</b> ";
    [SerializeField] private Color nameColor = Color.black;
    [SerializeField] private bool useColorForName = true;

    [Header("Skip Settings")]
    [SerializeField] private bool allowSkip = true;
    [SerializeField] private bool skipRevealsInstantly = true;

    private Vector2 windowSize = Vector2.zero;
    private DialoguePart _currentDialogue;
    private Coroutine _coroutine;
    
    private bool _isRevealing = false;
    private bool _skipRequested = false;
    private Queue<DialoguePart> _dialogueQueue = new Queue<DialoguePart>();

    private void Start()
    {
        windowSize.x = rect.sizeDelta.x;
    }

    public void ShowDialogue(DialoguePart dialoguePart)
    {
        if (!dialoguePart) return;

        // Проверяем, является ли диалог принудительным
        if (dialoguePart.IsForcedDialogue)
        {
            ForceShowDialogue(dialoguePart);
            return;
        }

        // Если уже показываем текст ИЛИ есть очередь
        if (_isRevealing || _dialogueQueue.Count > 0)
        {
            Debug.Log($"[Dialogue] Adding to queue: {dialoguePart.name}, Queue size before: {_dialogueQueue.Count}");
            
            if (allowSkip && skipRevealsInstantly && _isRevealing)
            {
                // Запросить немедленное завершение текущего текста
                _skipRequested = true;
            }
            
            // Добавить в очередь
            _dialogueQueue.Enqueue(dialoguePart);
            return;
        }

        // Показать новый диалог
        StartShowingDialogue(dialoguePart);
    }

    public void ForceShowDialogue(DialoguePart dialoguePart)
    {
        if (!dialoguePart) return;

        Debug.Log($"[Dialogue] Force showing: {dialoguePart.name}, Current queue size: {_dialogueQueue.Count}");

        // Останавливаем текущую корутину
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
        
        // НЕ сбрасываем _isRevealing здесь - оставляем true!
        // Это предотвратит добавление новых диалогов напрямую
        _skipRequested = false;
        
        // Немедленно запускаем новый диалог
        StartShowingDialogue(dialoguePart);
    }

    private void StartShowingDialogue(DialoguePart dialoguePart)
    {
       // Debug.Log($"[Dialogue] Starting: {dialoguePart.name}, Queue size: {_dialogueQueue.Count}");
        
        // Убеждаемся что окно активно
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        _currentDialogue = dialoguePart;
        _skipRequested = false;
        
        // Останавливаем предыдущую корутину если она есть
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
        }
        
        _coroutine = StartCoroutine(RevealText(dialoguePart.Dialogue));
    }

    private IEnumerator RevealText(string text, float delay = -1f)
    {
        _isRevealing = true; // Устанавливаем в начале корутины
        
        if (delay < 0) delay = letterDelay;

        // Get speaker name
        string speakerName = _currentDialogue?.Character?.CharacterName ?? "Unknown";
        string formattedName = FormatSpeakerName(speakerName);

        // Show speaker name immediately
        _mainText.text = formattedName;

        // Small pause after showing name
        yield return new WaitForSeconds(0.1f);

        // Reveal dialogue text letter by letter
        for (int i = 0; i <= text.Length; i++)
        {
            // Проверка на запрос пропуска
            if (_skipRequested)
            {
                i = text.Length;
            }

            _mainText.text = formattedName + text.Substring(0, i);
            
            // Update window size
            windowSize.y = _mainText.preferredHeight + textPadding;
            rect.sizeDelta = windowSize;

            if (i < text.Length)
            {
                yield return new WaitForSeconds(delay);
            }
            else
            {
                // Текст полностью показан
                if (!_skipRequested)
                {
                    yield return new WaitForSeconds(fullTextDuration);
                }
                
                _isRevealing = false; // Сбрасываем только здесь
                _coroutine = null;
                AdvanceText();
            }
        }
    }

    private string FormatSpeakerName(string speakerName)
    {
        string formattedName = string.Format(nameFormat, speakerName);

        if (useColorForName)
        {
            string colorHex = ColorUtility.ToHtmlStringRGBA(nameColor);
            formattedName = $"<color=#{colorHex}>{formattedName}</color>";
        }

        return formattedName;
    }

    private void AdvanceText()
    {
       // Debug.Log($"[Dialogue] Advancing, Queue size: {_dialogueQueue.Count}");
        
        // Проверяем очередь сначала
        if (_dialogueQueue.Count > 0)
        {
            DialoguePart nextInQueue = _dialogueQueue.Dequeue();
           // Debug.Log($"[Dialogue] Showing from queue: {nextInQueue.name}");
            StartShowingDialogue(nextInQueue);
            return;
        }

        // Затем проверяем следующий диалог в цепочке
        if (_currentDialogue != null && _currentDialogue.NextDialogue)
        {
           // Debug.Log($"[Dialogue] Showing next in chain: {_currentDialogue.NextDialogue.name}");
            StartShowingDialogue(_currentDialogue.NextDialogue);
        }
        else
        {
           // Debug.Log("[Dialogue] No more dialogues, hiding window");
            // Только здесь отключаем окно
            gameObject.SetActive(false);
        }
    }

    public void SkipCurrentText()
    {
        if (_isRevealing && allowSkip)
        {
            _skipRequested = true;
        }
    }

    public bool IsRevealing => _isRevealing;

    public int QueueSize => _dialogueQueue.Count;

    public void ClearQueue()
    {
        _dialogueQueue.Clear();
    }

    private void OnDisable()
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
        _isRevealing = false;
        _dialogueQueue.Clear();
    }
}