using System;
using Arcatech.EventBus;
using UnityEngine;

namespace Arcatech.Audio
{
    public readonly struct AudioCall : IEvent
    {
        public readonly AudioCallType Type;
        public readonly SoundDefinition Sound;
        public readonly AudioCategory? CategoryOverride;
        public readonly Vector3? Position;
        public readonly Transform Parent;
        public readonly float Fade;
        public readonly float VolumeScale;
        public readonly float? PitchOverride;
        public readonly SoundHandle HandleToStop;
        public readonly float Value;      // громкость 0..1 ИЛИ длительность кроссфейда
        public readonly bool BoolValue;   // mute on/off
        public readonly Action<SoundHandle> OnPlayed;

        private AudioCall(AudioCallType type, SoundDefinition sound = null, AudioCategory? category = null,
            Vector3? position = null, Transform parent = null, float fade = 0f, float volumeScale = 1f,
            float? pitchOverride = null, SoundHandle handleToStop = default, float value = 0f,
            bool boolValue = false, Action<SoundHandle> onPlayed = null)
        {
            Type = type; Sound = sound; CategoryOverride = category; Position = position;
            Parent = parent; Fade = fade; VolumeScale = volumeScale; PitchOverride = pitchOverride;
            HandleToStop = handleToStop; Value = value; BoolValue = boolValue; OnPlayed = onPlayed;
        }

        public static AudioCall Play(SoundDefinition sound, Vector3? position = null, Transform parent = null,
            AudioCategory? categoryOverride = null, float volumeScale = 1f, float? pitchOverride = null,
            Action<SoundHandle> onPlayed = null)
            => new(AudioCallType.Play, sound, categoryOverride, position, parent,
                volumeScale: volumeScale, pitchOverride: pitchOverride, onPlayed: onPlayed);

        public static AudioCall PlayUi(SoundDefinition sound, Action<SoundHandle> onPlayed = null)
            => new(AudioCallType.Play, sound, AudioCategory.Ui, onPlayed: onPlayed);

        public static AudioCall Music(SoundDefinition track, float crossfade = 1f)
            => new(AudioCallType.PlayMusic, track, value: crossfade);

        public static AudioCall Stop(SoundHandle handle, float fade = 0f)
            => new(AudioCallType.Stop, handleToStop: handle, fade: fade);

        public static AudioCall StopAllOf(AudioCategory? category = null, float fade = 0f)
            => new(AudioCallType.StopAll, category: category, fade: fade);

        public static AudioCall Volume(AudioCategory category, float linear01)
            => new(AudioCallType.SetCategoryVolume, category: category, value: linear01);

        public static AudioCall Mute(AudioCategory category, bool mute)
            => new(AudioCallType.SetMute, category: category, boolValue: mute);
    }
}