using Arcatech.Managers;
using System.Collections;
using UnityEngine;

namespace Arcatech.Effects
{
    /// <summary>
    /// stores sound clip data
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SoundEmitter : MonoBehaviour
    {
        public SoundClipData Data { get; private set; }

        AudioSource audioSource;
        Coroutine playingCor;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }
        public void Initialize (SoundClipData data)
        {
            audioSource.clip = data.clip;
            audioSource.outputAudioMixerGroup = data.mixerGroup;
            audioSource.loop = data.loop;
            audioSource.playOnAwake = data.playOnAwake;
            Data = data;
        }

        public void Play()
        {
            if (playingCor != null) StopCoroutine(playingCor);

            audioSource.Play();
            playingCor = StartCoroutine(WaitForEndSound());
        }

        private IEnumerator WaitForEndSound()
        {
            yield return new WaitWhile(()=>audioSource.isPlaying);
            EffectsManager.Instance.ReturnSound(this);
        }
        public void Stop ()
        {
            if (playingCor != null)
            {
                StopCoroutine(playingCor);
                playingCor = null;
            }
            audioSource.Stop();
            EffectsManager.Instance.ReturnSound(this);
        }

        internal void WithRandomPitch(float min = -0.05f, float max = 0.05f)
        {
            audioSource.pitch += UnityEngine.Random.Range(min, max);
        }
    }
}