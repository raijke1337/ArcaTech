using Arcatech.EventBus;
using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Arcatech.Effects
{
    [Serializable]
    public class SoundClipData
    {
        public AudioClip clip;
        public AudioMixerGroup mixerGroup;
        public bool loop;
        public bool playOnAwake;

    }
    public class SoundClipRequest : IEvent
    {
        public readonly SoundClipData Data;
        public readonly bool RandomPitch;
        public readonly Vector3 Place;

        public SoundClipRequest(SoundClipData data, bool randomPitch, Vector3 place)
        {
            Data = data;
            RandomPitch = randomPitch;
            Place = place;
        }
    }
}