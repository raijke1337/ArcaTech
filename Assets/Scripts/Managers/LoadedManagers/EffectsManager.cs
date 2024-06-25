using Arcatech.Effects;
using Arcatech.EventBus;
using CartoonFX;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Arcatech.Managers
{
    public class EffectsManager : MonoBehaviour
    {
        #region singleton
        public static EffectsManager Instance;
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); }
        }
        #endregion
        private void Start()
        {
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
            _drawDamageEventBind = new EventBinding<DrawDamageEvent>(PlaceDamageText);
            EventBus<DrawDamageEvent>.Register(_drawDamageEventBind);
        }

        private void PlaceDamageText(DrawDamageEvent @event)
        {
            Debug.Log($"Place damage text {@event.Damage} at {@event.Unit.transform.position}");
        }

        private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
        {
            PlayMusic(GameManager.Instance.GetCurrentLevelData.Music);
        }




        [Header("Sound prefabs")]
        [SerializeField] private AudioSource _audioPrefab;
        [SerializeField] private AudioSource _musicPrefab;
        [Header("Effect prefabs")]
        [SerializeField] private CFXR_ParticleText _particleTextPrefab;
        private EventBinding<DrawDamageEvent> _drawDamageEventBind;
        

        private AudioSource _musicObj;


        public void ServeEffectsRequest(EffectRequestPackage pack)
        {


            if (pack.Sound != null)
            {
                PlaceSound(pack.Sound,pack.Place);
              //  Debug.Log($"Serving effect request {pack.Sound} at {pack.Place}");
            }
            if (pack.Effect != null)
            {
                // Debug.Log($"Serving effect request {pack.Effect} at {pack.Place}");
                PlaceParticle(pack.Effect,pack.Place,pack.Parent);
            }
        }


        private void PlaceSound(AudioClip clip, Transform place)
        {
            var s = Instantiate(_audioPrefab, place.position, Quaternion.identity, transform);
            s.clip = clip;
            //s.volume *= SoundMixerManager.Instance.GetSFXVolume;

            s.Play();

            Destroy(s.gameObject, s.clip.length);
        }
        private void PlaceParticle(CFXR_Effect eff, Transform place, Transform parent = null)
        {
            var p = Instantiate(eff, place.position, place.rotation);
            if (parent != null)
            {
                p.transform.SetParent(parent, true);
            }
        }

        private void PlayMusic(AudioClip clip)
        {
            if (clip == null) return;
            _musicObj = Instantiate(_musicPrefab);
            _musicObj.clip = clip;
            _musicObj.loop = true;
            _musicObj.Play();
        }


        public void CleanUpOnSceneChange()
        {
            StopAllCoroutines();
            if (_musicObj != null) Destroy(_musicObj.gameObject);
        }

    }
}