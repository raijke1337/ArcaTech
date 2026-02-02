using System.Collections.Generic;
using System.Linq;
using Arcatech.Effects;
using Arcatech.EventBus;
using CartoonFX;
using UnityEngine;
using UnityEngine.Pool;

namespace Arcatech.Managers
{
    public partial class EffectsManager : GenericLazySingleton<EffectsManager>
    {
        protected override void Awake()
        {
            base.Awake();
            // Prewarm configured prefabs
            for (int i = 0; i < prewarm.Count; i++)
            {
                var entry = prewarm[i];
                if (entry.prefab == null || entry.initial <= 0) continue;

                if (!_particlesPools.ContainsKey(entry.prefab))
                    _particlesPools[entry.prefab] = new Queue<PooledEffect>();

                maxPerPrefab[entry.prefab] = entry.max > 0 ? entry.max : int.MaxValue;

                for (int j = 0; j < entry.initial; j++)
                {
                    var ins = CreateInstance(entry.prefab);
                }
            }
        }

        #region particles

        private EventBinding<ParticlesEvent> _placeParticleEventBind;

        [Header("Optional prewarm")] public List<PrewarmEntry> prewarm = new List<PrewarmEntry>();

        // Pools keyed by prefab asset
        readonly Dictionary<CFXR_Effect, Queue<PooledEffect>>
            _particlesPools = new();

        // Optional caps
        readonly Dictionary<CFXR_Effect, int> maxPerPrefab = new();


        private void HandleEvent(ParticlesEvent request)
        {
            if (request.Effect == null) return; // lazy ass guard
            Spawn(request.Effect, request.Place, Quaternion.identity, request.Parent);
        }

        private void Spawn(CFXR_Effect prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (prefab == null)
            {
                Debug.LogWarning("[EffectsManager] Spawn called with null prefab.");
                return;
            }

            if (!_particlesPools.TryGetValue(prefab, out var pool))
            {
                pool = new Queue<PooledEffect>();
                _particlesPools[prefab] = pool;
            }

            PooledEffect inst = pool.Count > 0 ? pool.Dequeue() : CreateInstance(prefab);
            // Parent and position
            if (!inst) return;
            // sometimes null - FIX
            
            var t = inst.transform;
            if (parent != null && t.gameObject.activeInHierarchy)
            {
                t.SetParent(parent, false);
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
            }
            else
            {
                t.SetParent(transform, false);
            }

            t.SetPositionAndRotation(position, rotation); // if position is world-space

            // Ensure clean state and play
            inst.gameObject.SetActive(true);
            inst.PrepareForPlay();
            inst.PlayNow();

        }

    public void Return(PooledEffect inst)
        {
            if (inst == null) return;

           // inst.transform.SetParent(transform, false);
            
            // Clean up
            inst.gameObject.SetActive(false);
            var prefab = inst.prefabKey;
            if (!_particlesPools.TryGetValue(prefab, out var pool))
            {
                pool = new Queue<PooledEffect>();
                _particlesPools[prefab] = pool;
            }

            // Optional: enforce max pool size per prefab
            if (maxPerPrefab.TryGetValue(prefab, out int max) && pool.Count >= max)
            {
                Destroy(inst.gameObject);
                return;
            }

            pool.Enqueue(inst);
        }
        
        
        PooledEffect CreateInstance(CFXR_Effect prefab)
        {
            var go = Instantiate(prefab, transform);
            go.name = $"{prefab.name} (Pooled)";
            go.clearBehavior = CFXR_Effect.ClearBehavior.None;
            var pooled = go.GetComponent<PooledEffect>();
            if (pooled == null) pooled = go.gameObject.AddComponent<PooledEffect>();

            pooled.owner = this;
            pooled.prefabKey = prefab;
            go.gameObject.SetActive(false);
            return pooled;
        }

        #endregion
        
        
        private EventBinding<SoundClipRequest> _playSoundEventBind;

        private void Start()
        {
            InitSoundPool();
            _placeParticleEventBind = new EventBinding<ParticlesEvent>(HandleEvent);
            EventBus<ParticlesEvent>.Register(_placeParticleEventBind);
            
        }
        private void OnDisable()
        {
            StopAllCoroutines();
            EventBus<ParticlesEvent>.Deregister(_placeParticleEventBind);
            EventBus<SoundClipRequest>.Deregister(_playSoundEventBind);
        }




        #region sound fx

        IObjectPool<SoundEmitter> soundsPool;
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
            soundsPool = new ObjectPool<SoundEmitter>(

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
            return soundsPool.Get();
        }
        public void ReturnSound(SoundEmitter s)
        {
            soundsPool.Release(s);
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


    }
}