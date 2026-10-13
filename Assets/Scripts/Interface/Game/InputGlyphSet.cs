using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Arcatech.UI
{
    /// <summary>Семейство иконок кнопок.</summary>
    public enum InputGlyphFamily
    {
        KeyboardMouse,
        Xbox,
        PlayStation
    }

    /// <summary>
    /// Иконки кнопок для подсказок.
    ///
    /// Иконка подбирается по ФИЗИЧЕСКОЙ кнопке, а не по действию, поэтому при смене биндов
    /// в PlayerControls ничего перенастраивать не нужно.
    ///
    /// Порядок поиска:
    ///  1. Entries - ручные переопределения (обычно пусто);
    ///  2. встроенная таблица под "all input icons same size" (Xbox, PlayStation, мышь, особые клавиши);
    ///  3. для клавиатуры - спрайт с именем самой клавиши (f, q, 1, f5...);
    ///  4. если спрайта нет в Sprite Asset - текст по Fallback Format, например [F].
    ///
    /// Проверить, всем ли биндам нашлись иконки: ⋮ (меню компонента) -> "Проверить бинды PlayerControls".
    /// </summary>
    [CreateAssetMenu(fileName = "input_glyphs", menuName = "Game/UI/Input Glyph Set")]
    public class InputGlyphSet : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public InputGlyphFamily family;
            [Tooltip("Путь контрола без устройства: f, space, leftButton, buttonWest, dpad/left, leftShoulder")]
            public string controlPath;
            [Tooltip("Имя спрайта в Sprite Asset")]
            public string spriteName;
        }

        [Tooltip("Sprite Asset со всеми иконками.")]
        [SerializeField] private TMP_SpriteAsset spriteAsset;

        [Tooltip("Формат текста, если иконки нет. {0} - название клавиши.")]
        [SerializeField] private string fallbackFormat = "<b>[{0}]</b>";

        [Tooltip("Ручные переопределения. Нужны, только если встроенная таблица выбрала не ту иконку.")]
        [SerializeField] private List<Entry> entries = new();

        public TMP_SpriteAsset SpriteAsset => spriteAsset;
        public string FallbackFormat => string.IsNullOrEmpty(fallbackFormat) ? "[{0}]" : fallbackFormat;

        #region Встроенная таблица

        // Общие для всех геймпадов (в ассете одни и те же иконки).
        private static readonly Dictionary<string, string> GamepadCommon = Map(
            ("leftShoulder", "lb"), ("rightShoulder", "rb"),
            ("leftTrigger", "lt"), ("rightTrigger", "rt"),
            ("leftStick", "jl"), ("rightStick", "jr"),
            ("leftStickPress", "jlclick"), ("rightStickPress", "jrclick"),
            ("dpad", "dpad"), ("dpad/up", "du"), ("dpad/down", "dd"),
            ("dpad/left", "dl"), ("dpad/right", "dr"));

        private static readonly Dictionary<string, string> Xbox = Map(
            ("buttonSouth", "xa"), ("buttonEast", "xb"), ("buttonWest", "xx"), ("buttonNorth", "xy"),
            ("start", "xmenu"), ("select", "xwin"));

        private static readonly Dictionary<string, string> PlayStation = Map(
            ("buttonSouth", "px"), ("buttonEast", "po"), ("buttonWest", "ps"), ("buttonNorth", "pt"),
            ("start", "pmenu"), ("select", "pshare"));

        // Клавиши, у которых имя контрола не совпадает с именем спрайта.
        // Остальные (буквы, цифры, f1-f12, space, tab, enter, home, end...) находятся по своему имени.
        private static readonly Dictionary<string, string> KeyboardMouse = Map(
            ("leftButton", "ml"), ("rightButton", "mr"), ("middleButton", "mm"),
            ("scroll", "mm"), ("position", "mouse"), ("delta", "mouse"),
            ("leftShift", "shift"), ("rightShift", "shift"),
            ("leftCtrl", "ctrl"), ("rightCtrl", "ctrl"),
            ("leftAlt", "alt"), ("rightAlt", "alt"),
            ("leftMeta", "win"), ("rightMeta", "win"), ("leftWindows", "win"), ("rightWindows", "win"),
            ("escape", "esc"), ("delete", "del"), ("insert", "ins"),
            ("capsLock", "capslock"), ("numLock", "numlock"), ("printScreen", "prtscrn"),
            ("pageUp", "pageup"), ("pageDown", "pagedown"), ("numpadEnter", "enter"),
            ("upArrow", "ku"), ("downArrow", "kd"), ("leftArrow", "kl"), ("rightArrow", "kr"),
            ("backquote", "~"), ("minus", "-"), ("equals", "+"),
            ("leftBracket", "["), ("rightBracket", "]"), ("semicolon", ";"),
            ("comma", "less"), ("period", "more"), ("numpadMultiply", "asterisk"),
            ("numpadPlus", "+"), ("numpadMinus", "-"), ("contextMenu", "pmenu"));

        private static Dictionary<string, string> Map(params (string path, string sprite)[] pairs)
        {
            var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (path, sprite) in pairs) d[path] = sprite;
            return d;
        }

        #endregion

        private Dictionary<(InputGlyphFamily, string), string> _overrides;

        public bool TryGetSprite(InputGlyphFamily family, string controlPath, out string spriteName)
        {
            spriteName = null;
            if (spriteAsset == null || string.IsNullOrEmpty(controlPath)) return false;

            // 1. Ручные переопределения.
            BuildOverrides();
            if (_overrides.TryGetValue((family, controlPath.ToLowerInvariant()), out string manual) && HasSprite(manual))
            {
                spriteName = manual;
                return true;
            }

            // 2. Встроенная таблица.
            string builtIn = null;
            switch (family)
            {
                case InputGlyphFamily.Xbox:
                    if (!Xbox.TryGetValue(controlPath, out builtIn)) GamepadCommon.TryGetValue(controlPath, out builtIn);
                    break;
                case InputGlyphFamily.PlayStation:
                    if (!PlayStation.TryGetValue(controlPath, out builtIn)) GamepadCommon.TryGetValue(controlPath, out builtIn);
                    break;
                default:
                    KeyboardMouse.TryGetValue(controlPath, out builtIn);
                    break;
            }

            if (builtIn != null && HasSprite(builtIn))
            {
                spriteName = builtIn;
                return true;
            }

            // 3. Клавиатура: спрайт назван так же, как клавиша (f, q, 1, f5, space...).
            if (family == InputGlyphFamily.KeyboardMouse)
            {
                string byName = controlPath.ToLowerInvariant();
                if (HasSprite(byName))
                {
                    spriteName = byName;
                    return true;
                }
            }

            return false;
        }

        private bool HasSprite(string name) =>
            !string.IsNullOrEmpty(name) && spriteAsset.GetSpriteIndexFromName(name) >= 0;

        private void BuildOverrides()
        {
            if (_overrides != null) return;
            _overrides = new Dictionary<(InputGlyphFamily, string), string>();
            foreach (var e in entries)
            {
                if (string.IsNullOrEmpty(e.controlPath) || string.IsNullOrEmpty(e.spriteName)) continue;
                _overrides[(e.family, e.controlPath.Trim().ToLowerInvariant())] = e.spriteName.Trim();
            }
        }

        private void OnValidate() => _overrides = null;
        private void OnEnable() => _overrides = null;

        /// <summary>Проходит по всем биндам PlayerControls и пишет в консоль, для каких кнопок нет иконки.
        /// Запускать после изменения управления.</summary>
        [ContextMenu("Проверить бинды PlayerControls")]
        private void ValidateAgainstPlayerControls()
        {
            if (spriteAsset == null)
            {
                Debug.LogWarning($"{name}: не назначен Sprite Asset.", this);
                return;
            }

            var controls = new PlayerControls();
            var report = new StringBuilder();
            int ok = 0, missing = 0;

            foreach (InputAction action in controls.asset)
            {
                foreach (InputBinding binding in action.bindings)
                {
                    if (binding.isComposite || string.IsNullOrEmpty(binding.effectivePath)) continue;

                    string path = binding.effectivePath;
                    int slash = path.IndexOf('/');
                    string control = slash >= 0 ? path.Substring(slash + 1) : path;

                    InputGlyphFamily[] families = HasGroup(binding, InputPromptFormatter.GamepadGroup)
                        ? new[] { InputGlyphFamily.Xbox, InputGlyphFamily.PlayStation }
                        : new[] { InputGlyphFamily.KeyboardMouse };

                    foreach (var family in families)
                    {
                        if (TryGetSprite(family, control, out string sprite))
                        {
                            ok++;
                            report.AppendLine($"  ✓ {action.actionMap.name}/{action.name}: {family} {control} -> {sprite}");
                        }
                        else
                        {
                            missing++;
                            report.AppendLine($"  ✗ {action.actionMap.name}/{action.name}: {family} {control} -> нет иконки, будет текст");
                        }
                    }
                }
            }

            controls.Dispose();

            string header = $"{name}: иконки найдены для {ok}, не найдены для {missing}.";
            if (missing > 0) Debug.LogWarning(header + "\n" + report, this);
            else Debug.Log(header + "\n" + report, this);
        }

        private static bool HasGroup(InputBinding binding, string group)
        {
            if (string.IsNullOrEmpty(binding.groups)) return false;
            foreach (string g in binding.groups.Split(';'))
                if (string.Equals(g, group, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
    }
}
