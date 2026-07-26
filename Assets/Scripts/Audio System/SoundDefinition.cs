using UnityEngine;
using UnityEngine.Audio;

namespace Arcatech.Audio
{
    [CreateAssetMenu(fileName = "SO_Sound_", menuName = "Audio/Sound Definition")]
    public class SoundDefinition : ScriptableObject
    {
        [Header("Resource")]
        [Tooltip("AudioClip или AudioRandomContainer — рандомизация клипов/питча настраивается внутри самого ассета")]
        public AudioResource resource;

        [Header("Mixing")] public AudioCategory category = AudioCategory.Sfx;
        [Range(0f, 1f)] public float volume = 1f;
        public bool loop;

        [Header("Pause")]
        [Tooltip("Звук будет продолжать играть, даже если игра на паузе (клики UI, тикающие часы и т.п.)")]
        public bool ignorePause;

        [Header("3D")] public bool is3D;
        public float minDistance = 1f;
        public float maxDistance = 25f;
    }
}