using System.Collections.Generic;
using Arcatech.Effects;
using Arcatech.EventBus;
using CartoonFX;
using UnityEngine;
using UnityEngine.Pool;

namespace Arcatech.Managers
{
    public class EffectsManager : GenericLazySingleton<EffectsManager>
    {
        protected void Awake()
        {
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
        
     
        private void Start()
        {
            _placeParticleEventBind = new EventBinding<ParticlesEvent>(HandleEvent);
            EventBus<ParticlesEvent>.Register(_placeParticleEventBind);
        }
        
        private void OnDisable()
        {
            StopAllCoroutines();
            EventBus<ParticlesEvent>.Deregister(_placeParticleEventBind);
        }

    }
}