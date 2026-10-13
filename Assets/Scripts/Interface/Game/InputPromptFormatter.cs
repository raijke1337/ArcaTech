using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Arcatech.Units.Control;
using UnityEngine.InputSystem;

namespace Arcatech.UI
{
    /// <summary>
    /// Подставляет в текст кнопки действий под текущее устройство.
    /// "Нажми {Interact}, чтобы взломать" -> "Нажми [F], чтобы взломать" / "Нажми [X] ..." / иконка спрайтом.
    /// Токен - имя действия из PlayerControls: {Interact}, {Jump}, {SpecialMelee}
    /// или с картой действий: {Minigame/ButtonN}. Неизвестные токены остаются как есть.
    /// </summary>
    public static class InputPromptFormatter
    {
        // Имена групп привязок (control schemes) из PlayerControls.inputactions.
        public const string KeyboardMouseGroup = "KBM";
        public const string GamepadGroup = "GamePad";

        private static readonly Regex Token = new(@"\{([\w/]+)\}", RegexOptions.Compiled);

        // Запасной ассет, если игрока на сцене нет (например, подсказки в меню).
        private static PlayerControls _fallbackControls;

        public static string Format(string text, InputActionAsset actions, InputDeviceKind kind,
            InputDevice device, InputGlyphSet glyphs)
        {
            if (string.IsNullOrEmpty(text) || text.IndexOf('{') < 0) return text;

            if (actions == null)
            {
                _fallbackControls ??= new PlayerControls();
                actions = _fallbackControls.asset;
            }

            return Token.Replace(text, match =>
            {
                InputAction action = actions.FindAction(match.Groups[1].Value);
                if (action == null) return match.Value;
                return DescribeAction(action, kind, device, glyphs) ?? match.Value;
            });
        }

        private static string DescribeAction(InputAction action, InputDeviceKind kind, InputDevice device,
            InputGlyphSet glyphs)
        {
            string group = kind == InputDeviceKind.Gamepad ? GamepadGroup : KeyboardMouseGroup;
            var bindings = action.bindings;

            for (int i = 0; i < bindings.Count; i++)
            {
                InputBinding binding = bindings[i];
                if (binding.isPartOfComposite) continue;

                if (binding.isComposite)
                {
                    // Например OneModifier: "LB + A".
                    var parts = new List<string>();
                    for (int j = i + 1; j < bindings.Count && bindings[j].isPartOfComposite; j++)
                    {
                        if (InGroup(bindings[j], group))
                            parts.Add(DescribeControl(bindings[j].effectivePath, kind, device, glyphs));
                    }

                    if (parts.Count > 0) return string.Join(" + ", parts);
                    continue;
                }

                if (InGroup(binding, group))
                    return DescribeControl(binding.effectivePath, kind, device, glyphs);
            }

            return null;
        }

        private static string DescribeControl(string path, InputDeviceKind kind, InputDevice device,
            InputGlyphSet glyphs)
        {
            InputGlyphFamily family = GetFamily(kind, device);
            string controlPath = StripDevice(path);

            if (glyphs != null && glyphs.TryGetSprite(family, controlPath, out string sprite))
                return $"<sprite name=\"{sprite}\">";

            string label = GetLabel(path, kind, device);
            return string.Format(glyphs != null ? glyphs.FallbackFormat : "[{0}]", label);
        }

        /// <summary>Название клавиши с учётом реального устройства:
        /// раскладка клавиатуры (AZERTY и т.п.), "Square" на DualSense вместо "X".</summary>
        private static string GetLabel(string path, InputDeviceKind kind, InputDevice device)
        {
            InputControl control = null;

            if (kind == InputDeviceKind.Gamepad)
            {
                InputDevice pad = device as Gamepad ?? Gamepad.current;
                if (pad != null) control = InputControlPath.TryFindControl(pad, path);
            }
            else
            {
                if (Keyboard.current != null) control = InputControlPath.TryFindControl(Keyboard.current, path);
                if (control == null && Mouse.current != null) control = InputControlPath.TryFindControl(Mouse.current, path);
            }

            if (control != null)
            {
                if (!string.IsNullOrEmpty(control.shortDisplayName)) return control.shortDisplayName;
                if (!string.IsNullOrEmpty(control.displayName)) return control.displayName;
            }

            return InputControlPath.ToHumanReadableString(path,
                InputControlPath.HumanReadableStringOptions.OmitDevice |
                InputControlPath.HumanReadableStringOptions.UseShortNames);
        }

        private static InputGlyphFamily GetFamily(InputDeviceKind kind, InputDevice device)
        {
            if (kind != InputDeviceKind.Gamepad) return InputGlyphFamily.KeyboardMouse;

            InputDevice pad = device as Gamepad ?? Gamepad.current;
            if (pad != null && InputSystem.IsFirstLayoutBasedOnSecond(pad.layout, "DualShockGamepad"))
                return InputGlyphFamily.PlayStation;

            return InputGlyphFamily.Xbox;
        }

        private static bool InGroup(InputBinding binding, string group)
        {
            if (string.IsNullOrEmpty(binding.groups)) return false;
            foreach (string g in binding.groups.Split(';'))
            {
                if (string.Equals(g, group, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        // "<Gamepad>/buttonWest" -> "buttonWest", "<Gamepad>/dpad/left" -> "dpad/left"
        private static string StripDevice(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            int slash = path.IndexOf('/');
            return slash >= 0 ? path.Substring(slash + 1) : path;
        }
    }
}
