using System;
using System.Collections;
using Arcatech.Items;
using Arcatech.Texts;
using KBCore.Refs;
using DG.Tweening;
using System.Collections.Generic;
using com.cyborgAssets.inspectorButtonPro;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.UI
{
    public class GameTextWindowComponent : ValidatedMonoBehaviour
    {

        [Space, Header("Text settings"),SerializeField] private TextMeshProUGUI _mainText;
        [SerializeField] private float letterDelay = 0.1f;
        [SerializeField] private float fullTextDuration = 1.276f;
        [SerializeField, Self] private RectTransform rect;
        [SerializeField] private float textPadding = 10f;

        [Header("Speaker Name Formatting")] [SerializeField]
        private string nameFormat = "<b>{0}:</b> "; // {0} will be replaced with character name

        [SerializeField] private Color nameColor = Color.black;
        [SerializeField] private bool useColorForName = true;

        Vector2 windowSize = Vector2.zero;
        
        
        private DialoguePart _currentDialogue;
        Coroutine _coroutine;

        private void Start()
        {
            windowSize.x = rect.sizeDelta.x;
        }

        public void ShowDialogue(DialoguePart dialoguePart)
        {
            gameObject.SetActive(true);
            _currentDialogue = dialoguePart;
            _coroutine = StartCoroutine(RevealText(dialoguePart.Dialogue));
        }

        private void SetFonts()
        {
            _mainText.font = GameUIManager.Instance.GetFont(FontType.Text);
        }

        private IEnumerator RevealText(string text, float delay = -1f)
        {
            if (delay < 0) delay = letterDelay;

            // Get speaker name
            string speakerName = _currentDialogue?.Character?.CharacterName ?? "Unknown";

            // Format the speaker name
            string formattedName = string.Format(nameFormat, speakerName);

            // Apply color to name if enabled
            if (useColorForName)
            {
                string colorHex = ColorUtility.ToHtmlStringRGBA(nameColor);
                formattedName = $"<color=#{colorHex}>{formattedName}</color>";
            }

            // Show speaker name immediately
            _mainText.text = formattedName;

            // Small pause after showing name (optional)
            yield return new WaitForSeconds(0.1f);

            // Reveal dialogue text letter by letter
            for (int i = 0; i <= text.Length; i++)
            {
                _mainText.text = formattedName + text.Substring(0, i);
                windowSize.y = _mainText.preferredHeight+textPadding;
                rect.sizeDelta = windowSize;

                if (i < text.Length)
                    yield return new WaitForSeconds(delay);
                else
                {
                    yield return new WaitForSeconds(fullTextDuration);
                    AdvanceText();
                }
            }
            
        }
    
        void AdvanceText()
        {
            if (_currentDialogue.NextDialogue)
            {
                ShowDialogue(_currentDialogue.NextDialogue);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
        
        
        #if UNITY_EDITOR
        [SerializeField] DialoguePart debugDialogue;
        [ProButton]
        void LoadDebugText()
        {
            ShowDialogue(debugDialogue);
        }
        #endif
    }
}