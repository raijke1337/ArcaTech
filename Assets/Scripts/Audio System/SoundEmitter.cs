using System;
using System.Collections;
using Arcatech.Units;
using UnityEngine;
using UnityEngine.Audio;

namespace Arcatech.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundEmitter : MonoBehaviour, IPausableComponent
    {
        public int CurrentId { get; private set; }
        public AudioCategory Category { get; private set; }

        private AudioSource _source;
        private Action<SoundEmitter> _releaseToPool;
        private Coroutine _lifeRoutine;
        private bool _ignorePause;
        private bool _paused;

        public bool Paused
        {
            get => _paused;
            set
            {
                if (_ignorePause || _paused == value) return;
                _paused = value;
                if (_paused) _source.Pause();
                else _source.UnPause();
            }
        }

        private void Awake() => _source = GetComponent<AudioSource>();

        public void Init(Action<SoundEmitter> releaseCallback) => _releaseToPool = releaseCallback;

        public void Play(SoundDefinition def, int id, Vector3? position, AudioMixerGroup group,
            AudioCategory category, float volumeScale = 1f, float? pitchOverride = null)
        {
            CurrentId = id;
            Category = category;
            _ignorePause = def.ignorePause;
            _paused = false;

            if (position.HasValue) transform.position = position.Value;

            _source.resource = def.resource; // AudioClip или AudioRandomContainer — без разницы для API
            _source.outputAudioMixerGroup = group;
            _source.volume = def.volume * volumeScale;
            if (pitchOverride.HasValue) _source.pitch = pitchOverride.Value; // иначе питчом рулит сам ARC
            _source.loop = def.loop;
            _source.spatialBlend = def.is3D ? 1f : 0f;
            _source.minDistance = def.minDistance;
            _source.maxDistance = def.maxDistance;
            _source.Play();

            if (_lifeRoutine != null) StopCoroutine(_lifeRoutine);
            if (!def.loop)
                _lifeRoutine = StartCoroutine(ReleaseWhenFinished());
        }

        public void Stop(float fade)
        {
            if (fade <= 0f)
            {
                ReturnToPool();
                return;
            }

            StartCoroutine(FadeOutAndRelease(fade));
        }

        private IEnumerator ReleaseWhenFinished()
        {
            yield return null; // дать AudioSource кадр на "прогрев" isPlaying

            // Специально проверяем _paused сами, не надеясь на то, как именно
            // AudioSource.isPlaying ведёт себя во время Pause() в конкретной версии движка —
            // так поведение предсказуемо независимо от внутренних деталей Unity.
            while (_paused || _source.isPlaying)
                yield return null;

            ReturnToPool();
        }

        private IEnumerator FadeOutAndRelease(float duration)
        {
            float startVol = _source.volume;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                _source.volume = Mathf.Lerp(startVol, 0f, t / duration);
                yield return null;
            }

            ReturnToPool();
        }

        private void ReturnToPool()
        {
            if (_lifeRoutine != null) StopCoroutine(_lifeRoutine);
            _source.Stop();
            _source.pitch = 1f;
            CurrentId = 0;
            _paused = false;
            _releaseToPool?.Invoke(this);
        }
    }
}