using System.Collections;
using System.Collections.Generic;
using Arcatech.Actions;
using Arcatech.Effects;
using Arcatech.EventBus;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace Arcatech.Managers
{
    public partial class EffectsManager : MonoBehaviour
    {

        [Header("Visaul effects setings")]
        [SerializeField] private DamageTextContainer _particleTextPrefab;     

        private EventBinding<DrawDamageEvent> _drawDamageEventBind;
        private EventBinding<VFXRequest> _placeParticleEventBind;
        private EventBinding<SoundClipRequest> _playSoundEventBind;
        private EventBinding<ProjectilePlaceEvent> _projectilePlaceEventBind;

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

            InitSoundPool();

            _drawDamageEventBind = new EventBinding<DrawDamageEvent>(PlaceDamageText);
            _placeParticleEventBind = new EventBinding<VFXRequest>(PlaceParticle);
            _playSoundEventBind = new EventBinding<SoundClipRequest>(CreateSound);
            _projectilePlaceEventBind = new EventBinding<ProjectilePlaceEvent>(PlaceProjectile);


            EventBus<DrawDamageEvent>.Register(_drawDamageEventBind);
            EventBus<VFXRequest>.Register(_placeParticleEventBind);
            EventBus<SoundClipRequest>.Register(_playSoundEventBind);
            EventBus<ProjectilePlaceEvent>.Register(_projectilePlaceEventBind);

        }
        private void OnDisable()
        {
            StopAllCoroutines();
            EventBus<DrawDamageEvent>.Deregister(_drawDamageEventBind);
            EventBus<VFXRequest>.Deregister(_placeParticleEventBind);
            EventBus<SoundClipRequest>.Deregister(_playSoundEventBind);
            EventBus<ProjectilePlaceEvent>.Deregister(_projectilePlaceEventBind);
        }



        #region VFX
        private void PlaceParticle(VFXRequest request)
        {
            if (request.Effect == null) return;
            var ef = Instantiate(request.Effect, request.Place.position, request.Place.rotation);
            if (request.Parent != null) { ef.transform.SetParent(request.Parent.transform, true); }
        }

        private void PlaceDamageText(DrawDamageEvent @event)
        {
            if (@event.Unit == null) return;

            Vector3 dirToCamera = Camera.main.transform.position - @event.Unit.transform.position;
            Vector3 adjustedPosition = @event.Unit.transform.position + (Vector3.up * 2) + dirToCamera.normalized + Random.insideUnitSphere; // move towards camera 1 an d some random

            var txt = Instantiate(_particleTextPrefab, adjustedPosition, Quaternion.identity);

            txt.PlayNumbers((int)@event.Damage);
        }
        #endregion

        #region sound fx

        IObjectPool<SoundEmitter> pool;
        readonly List<SoundEmitter> active = new List<SoundEmitter>();
        //to stop all
        public readonly Dictionary<SoundClipData, int> Counts = new Dictionary<SoundClipData, int>();
        // how many instances of sound

        [Space,Header("Sound effect settings")]
        [SerializeField] SoundEmitter emitterPrefab;
        [SerializeField] int maxSoundInstances = 30;


        #region pool

        [Space, Header("Sound pool settings")]
        [SerializeField] bool collectionCheck = true;
        [SerializeField] int defaultCapacity = 10;
        [SerializeField] int maxSize = 100;


        private void CreateSound(SoundClipRequest obj)
        {
            SoundsBuilder b = new SoundsBuilder(this).WithSoundData(obj.Data)
                .WithPosition(obj.Place).
                WithRandomPitch(obj.RandomPitch);
            b.Play();
        }


        SoundEmitter CreateSoundEmitter()
        {
            var e = Instantiate(emitterPrefab);
            e.gameObject.SetActive(false);
            return e;
        }
        void OnTakeFromPool(SoundEmitter s)
        {
            s.gameObject.SetActive(true);
            active.Add(s);
        }

        void OnDestroyPoolObject(SoundEmitter obj)
        {
            Destroy(obj.gameObject);
        }

        void OnReturnedToPool(SoundEmitter obj)
        {
            if (Counts.TryGetValue(obj.Data, out int c))
            {
                Counts[obj.Data] -= c > 0 ? 1 : 0;
            }


            obj.gameObject.SetActive(false);
            active.Remove(obj);
        }

        void InitSoundPool()
        {
            pool = new ObjectPool<SoundEmitter>(

                CreateSoundEmitter,
                OnTakeFromPool,
                OnReturnedToPool,
                OnDestroyPoolObject,
                collectionCheck,
                defaultCapacity,
                maxSize);
        }
        #endregion

        #region public

        public SoundEmitter GetSound()
        {
            return pool.Get();
        }
        public void ReturnSound(SoundEmitter s)
        {
            pool.Release(s);
        }

        public bool CanPlaySound(SoundClipData data)
        {
            if (Counts.TryGetValue(data, out var count))
            {
                return count < maxSoundInstances;
            }
            else return true;
        }

        #endregion
        #endregion
        #region projectiles


        private void PlaceProjectile(ProjectilePlaceEvent p)
        {
            StartCoroutine(ShootingCoroutine(p));
        }

        IEnumerator ShootingCoroutine(ProjectilePlaceEvent ev)
        {

            Vector3 place = ev.Place.position;
            Quaternion rotation = ev.Place.rotation; // bandaid

            // should make the bandaid unnecessary
            if (ev.ShootingConfig.ShotDelay >0)
            {
                yield return new WaitForSeconds(ev.ShootingConfig.ShotDelay);
            }
            if (ev.Place == null) // place projectile was called on destroy, ref null, use cached spot bandaid
            {

            }
            else
            {
                 place = ev.Place.position;
                 rotation = ev.Place.rotation;
                // transform ref is not null
            }

            int done = 0;

            if (ev.Place == null)
            {
                while (done < ev.ShootingConfig.Shots)
                {
                    done++;
                    ev.Projectile.ProduceProjectile(ev.Shooter, place, rotation, ev.ShootingConfig.Spread);
                    yield return new WaitForSeconds(ev.ShootingConfig.BetweenShotsDelay);
                }
                yield return null;
            }
            else
            {
                while (done < ev.ShootingConfig.Shots)
                {
                    done++;
                    ev.Projectile.ProduceProjectile(ev.Shooter, place,rotation, ev.ShootingConfig.Spread);
                    yield return new WaitForSeconds(ev.ShootingConfig.BetweenShotsDelay);
                }
                yield return null;
            }
        }

        #endregion

    }
}