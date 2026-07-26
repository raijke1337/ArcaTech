using System;
using System.Collections;
using System.Collections.Generic;
using Arcatech.EventBus;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

namespace Arcatech.Audio
{
    public class AudioService : MonoBehaviour
    {
        [SerializeField] private SoundEmitter emitterPrefab;
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private int prewarmCount = 16;
        [SerializeField] private AudioCategoryGroup[] categoryGroups;

        [SerializeField] private SoundEmitter musicSlotA;
        [SerializeField] private SoundEmitter musicSlotB;
        private bool _usingA;

        private ObjectPool<SoundEmitter> _pool;
        private readonly List<SoundEmitter> _activeEmitters = new();
        private readonly Dictionary<AudioCategory, AudioMixerGroup> _groupByCategory = new();
        private readonly Dictionary<AudioCategory, float> _lastVolume = new();
        private int _nextId = 1;

        private EventBinding<AudioCall> _playSoundEventBind;

        private void Awake()
        {
            foreach (var g in categoryGroups)
                _groupByCategory[g.Category] = g.MixerGroup;

            _pool = new ObjectPool<SoundEmitter>(
                createFunc: CreateEmitter,
                actionOnGet: e => e.gameObject.SetActive(true),
                actionOnRelease: e => e.gameObject.SetActive(false),
                actionOnDestroy: e => Destroy(e.gameObject),
                collectionCheck: false,
                defaultCapacity: prewarmCount);

            for (int i = 0; i < prewarmCount; i++)
                _pool.Release(CreateEmitter());
        }

        private void Start()
        {
            _playSoundEventBind = new EventBinding<AudioCall>(HandleEvent);
            EventBus<AudioCall>.Register(_playSoundEventBind);
        }

        private void OnDestroy()
        {
            EventBus<AudioCall>.Deregister(_playSoundEventBind);
        }

        private SoundEmitter CreateEmitter()
        {
            var emitter = Instantiate(emitterPrefab, transform);
            emitter.Init(OnEmitterReleased);
            return emitter;
        }

        private void OnEmitterReleased(SoundEmitter emitter)
        {
            _activeEmitters.Remove(emitter);
            _pool.Release(emitter);
        }

        private void HandleEvent(AudioCall call)
        {
            switch (call.Type)
            {
                case AudioCallType.Play:
                    var handle = PlayInternal(call);
                    call.OnPlayed?.Invoke(handle);
                    break;

                case AudioCallType.PlayMusic:
                    StartCoroutine(CrossfadeMusic(call.Sound, call.Value));
                    break;

                case AudioCallType.Stop:
                    if (call.HandleToStop.IsValid) call.HandleToStop.Emitter.Stop(call.Fade);
                    break;

                case AudioCallType.StopAll:
                    StopAllInternal(call.CategoryOverride, call.Fade);
                    break;

                case AudioCallType.SetCategoryVolume:
                    SetCategoryVolumeInternal(call.CategoryOverride ?? AudioCategory.Master, call.Value);
                    break;

                case AudioCallType.SetMute:
                    SetMuteInternal(call.CategoryOverride ?? AudioCategory.Master, call.BoolValue);
                    break;
            }
        }

        private SoundHandle PlayInternal(AudioCall call)
        {
            var sound = call.Sound;
            if (sound == null) return default;

            var category = call.CategoryOverride ?? sound.category;
            var group = _groupByCategory.TryGetValue(category, out var g) ? g : null;

            var emitter = _pool.Get();
            emitter.transform.SetParent(call.Parent != null ? call.Parent : transform, false);

            int id = _nextId++;
            emitter.Play(sound, id, call.Position, group, category, call.VolumeScale, call.PitchOverride);
            _activeEmitters.Add(emitter);

            return new SoundHandle(id, emitter);
        }

        private void StopAllInternal(AudioCategory? category, float fade)
        {
            var snapshot = new List<SoundEmitter>(_activeEmitters);
            foreach (var emitter in snapshot)
                if (category == null || emitter.Category == category)
                    emitter.Stop(fade);
        }

        private void SetCategoryVolumeInternal(AudioCategory category, float linear01)
        {
            float db = linear01 <= 0.0001f ? -80f : Mathf.Log10(linear01) * 20f;
            mixer.SetFloat(category.ToString(), db);
            _lastVolume[category] = linear01;
            PlayerPrefs.SetFloat($"vol_{category}", linear01);
        }

        private void SetMuteInternal(AudioCategory category, bool mute)
        {
            if (mute)
            {
                if (!_lastVolume.ContainsKey(category)) _lastVolume[category] = 1f;
                mixer.SetFloat(category.ToString(), -80f);
            }
            else
            {
                SetCategoryVolumeInternal(category, _lastVolume.TryGetValue(category, out var v) ? v : 1f);
            }
        }

        private IEnumerator CrossfadeMusic(SoundDefinition track, float duration)
        {
            // логика как в предыдущей версии, детали пропущены
            yield return null;
        }
    }

    [Serializable]
    public struct AudioCategoryGroup
    {
        public AudioCategory Category;
        public AudioMixerGroup MixerGroup;
    }
}