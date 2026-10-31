using System;
using System.Collections.Generic;

namespace Arcatech.SaveSystem
{
    public enum DifficultyLevel
    {
        Easy,
        Normal,
        Hard
    }

    /// <summary>
    /// Режим контента: определяет, какие 18+ анимации могут проигрываться (в игре и в галерее).
    /// Значения зафиксированы явно, потому что enum пишется в settings.json числом.
    /// SFW-режима нет (решение дизайнера): остаются только Suggestive и Explicit.
    /// </summary>
    public enum ContentMode
    {
        Suggestive = 0,
        Explicit = 1
    }

    [Serializable]
    public abstract class VersionedSaveData
    {
        public int version;
        public string timestamp;
    }

    /// <summary>
    /// Технические настройки игры. Меняются часто (слайдеры громкости и т.п.),
    /// сохраняются с дебаунсом через SaveManager.
    /// Новые поля безопасно добавлять: при загрузке старого файла отсутствующие поля
    /// получают значения по умолчанию из инициализаторов ниже.
    /// Описание каждого поля: диздок «Настройки игры».
    /// </summary>
    [Serializable]
    public class GameSettingsData : VersionedSaveData
    {
        // ---------- Геймплей ----------
        public DifficultyLevel difficulty = DifficultyLevel.Normal;

        // ---------- Контент ----------
        /// <summary> Какие анимации могут проигрываться. По умолчанию Suggestive: Explicit включается игроком осознанно. </summary>
        public ContentMode contentMode = ContentMode.Suggestive;

        /// <summary>
        /// Режим стримера (отдельная галка). Не меняет набор доступных анимаций, его определяет contentMode.
        /// Включён: во всех suggestive/explicit анимациях показываются только силуэты.
        /// </summary>
        public bool streamerMode = false;

        // ---------- Звук (линейная громкость 0..1, по одному полю на AudioCategory) ----------
        public float masterVolume = 1f;
        public float musicVolume = 1f;
        public float sfxVolume = 1f;
        public float uiVolume = 1f;
        public float voiceVolume = 1f;
        public float ambientVolume = 1f;

        // ---------- Управление ----------
        // ключ = имя игрового действия ("Jump", "Fire" ...), значение = код кнопки/оси
        public Dictionary<string, string> keyBindings = new();
    }
}
