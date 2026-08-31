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

    [Serializable]
    public abstract class VersionedSaveData
    {
        public int version;
        public string timestamp;
    }

    /// <summary>
    /// Технические настройки игры. Меняются часто (слайдеры громкости и т.п.),
    /// сохраняются с дебаунсом через SaveManager.
    /// </summary>
    [Serializable]
    public class GameSettingsData : VersionedSaveData
    {
        public DifficultyLevel difficulty = DifficultyLevel.Normal;

        public float masterVolume = 1f;
        public float musicVolume = 1f;
        public float sfxVolume = 1f;

        // ключ = имя игрового действия ("Jump", "Fire" ...), значение = код кнопки/оси
        public Dictionary<string, string> keyBindings = new();
    }
}