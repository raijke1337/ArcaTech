using System;
using Arcatech.EventBus;
using UnityEngine.Audio;

namespace Arcatech.Audio
{
    public static class AudioEvents
    {
          
        public static void Play(SoundDefinition sound, UnityEngine.Vector3? position = null,
            UnityEngine.Transform parent = null, Action<SoundHandle> onPlayed = null)
            => EventBus<AudioCall>.Raise(AudioCall.Play(sound, position, parent, onPlayed: onPlayed));

        public static void PlayUi(SoundDefinition sound, Action<SoundHandle> onPlayed = null)
        {
            if (sound == null) return;
            EventBus<AudioCall>.Raise(AudioCall.PlayUi(sound, onPlayed));
        }

        public static void PlayMusic(SoundDefinition track, float crossfade = 1f)
            => EventBus<AudioCall>.Raise(AudioCall.Music(track, crossfade));

        public static void Stop(SoundHandle handle, float fade = 0f)
            => EventBus<AudioCall>.Raise(AudioCall.Stop(handle, fade));

        public static void StopAll(AudioCategory? category = null, float fade = 0f)
            => EventBus<AudioCall>.Raise(AudioCall.StopAllOf(category, fade));

        public static void SetVolume(AudioCategory category, float linear01)
            => EventBus<AudioCall>.Raise(AudioCall.Volume(category, linear01));

        public static void SetMute(AudioCategory category, bool mute)
            => EventBus<AudioCall>.Raise(AudioCall.Mute(category, mute));
    }
}

