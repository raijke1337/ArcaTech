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
        [Header("Pool")]
        [SerializeField, Min(0)]
        private int _defaultCapacity = 8;

        [SerializeField, Min(1)]
        private int _maxPoolSizePerEffect = 64;

        [SerializeField]
        private Transform _poolRoot;

        private readonly Dictionary<int, EffectPool> _pools = new();

        private EventBinding<ParticlesEvent> _particlesEventBinding;
        private bool _isRegistered;

        protected void Awake()
        {

            if (_poolRoot == null)
            {
                var rootObject = new GameObject("[Pooled Effects]");
                rootObject.transform.SetParent(transform);
                _poolRoot = rootObject.transform;
            }
        }

        private void OnEnable()
        {
            if (_isRegistered)
            {
                return;
            }

            _particlesEventBinding = new EventBinding<ParticlesEvent>(HandleEvent);
            EventBus<ParticlesEvent>.Register(_particlesEventBinding);

            _isRegistered = true;
        }

        private void OnDisable()
        {
            if (!_isRegistered)
            {
                return;
            }

            EventBus<ParticlesEvent>.Deregister(_particlesEventBinding);

            _isRegistered = false;
        }

        private void OnDestroy()
        {
            foreach (var pool in _pools.Values)
            {
                pool.Dispose();
            }

            _pools.Clear();
        }

        private void HandleEvent(ParticlesEvent particlesEvent)
        {
            if (particlesEvent.Prefab == null)
            {
                Debug.LogWarning(
                    $"{nameof(EffectsManager)} received {nameof(ParticlesEvent)} without a prefab.",
                    this);

                return;
            }

            var pool = GetOrCreatePool(particlesEvent.Prefab);
            var effect = pool.Get();

            PrepareEffect(effect, particlesEvent);
        }

        private EffectPool GetOrCreatePool(CFXR_Effect prefab)
        {
            int prefabId = prefab.GetInstanceID();

            if (_pools.TryGetValue(prefabId, out var pool))
            {
                return pool;
            }

            pool = new EffectPool(
                prefab,
                _poolRoot,
                _defaultCapacity,
                _maxPoolSizePerEffect);

            _pools.Add(prefabId, pool);

            return pool;
        }

        private static void PrepareEffect(
            CFXR_Effect effect,
            in ParticlesEvent particlesEvent)
        {
            var effectTransform = effect.transform;
            var particleSystem = effect.GetComponent<ParticleSystem>();
            var lifetime = effect.GetComponent<PooledEffect>();

            // На случай, если предыдущая жизнь эффекта завершалась вручную.
            lifetime.Disarm();

            particleSystem.Stop(
                withChildren: true,
                stopBehavior: ParticleSystemStopBehavior.StopEmittingAndClear);

            particleSystem.Clear(withChildren: true);

            effect.ResetState();

            if (particlesEvent.IsLocalSpace)
            {
                effectTransform.SetParent(particlesEvent.Parent, worldPositionStays: false);
                effectTransform.SetLocalPositionAndRotation(
                    particlesEvent.Position,
                    particlesEvent.Rotation);
            }
            else
            {
                // До активации эффект остаётся дочерним объектом пула,
                // но его координаты задаются в мировом пространстве.
                effectTransform.SetParent(null, worldPositionStays: false);
                effectTransform.SetPositionAndRotation(
                    particlesEvent.Position,
                    particlesEvent.Rotation);
            }

            effect.gameObject.SetActive(true);

            particleSystem.Play(withChildren: true);
            lifetime.Arm();
        }

        internal void Release(CFXR_Effect effect)
        {
            if (effect == null)
            {
                return;
            }

            var lifetime = effect.GetComponent<PooledEffect>();
            lifetime.Disarm();

            int prefabId = lifetime.PrefabInstanceId;

            if (_pools.TryGetValue(prefabId, out var pool))
            {
                pool.Release(effect);
            }
            else
            {
                Destroy(effect.gameObject);
            }
        }

        private sealed class EffectPool
        {
            private readonly CFXR_Effect _prefab;
            private readonly Transform _poolRoot;
            private readonly int _prefabInstanceId;

            private readonly ObjectPool<CFXR_Effect> _pool;

            public EffectPool(
                CFXR_Effect prefab,
                Transform poolRoot,
                int defaultCapacity,
                int maxPoolSize)
            {
                _prefab = prefab;
                _poolRoot = poolRoot;
                _prefabInstanceId = prefab.GetInstanceID();

                _pool = new ObjectPool<CFXR_Effect>(
                    createFunc: Create,
                    actionOnGet: OnGet,
                    actionOnRelease: OnRelease,
                    actionOnDestroy: OnDestroy,
                    collectionCheck: Application.isEditor,
                    defaultCapacity: defaultCapacity,
                    maxSize: maxPoolSize);
            }

            public CFXR_Effect Get()
            {
                return _pool.Get();
            }

            public void Release(CFXR_Effect effect)
            {
                _pool.Release(effect);
            }

            public void Dispose()
            {
                _pool.Dispose();
            }

            private CFXR_Effect Create()
            {
                var effect = Instantiate(_prefab, _poolRoot);
                effect.gameObject.SetActive(false);

                // Cartoon FX не должен самостоятельно выключать или уничтожать объект.
                // Возвратом в пул управляет PooledCfxEffectLifetime.
                effect.clearBehavior = CFXR_Effect.ClearBehavior.None;

                var particleSystem = effect.GetComponent<ParticleSystem>();

                // Unity вызовет OnParticleSystemStopped, когда root ParticleSystem завершится.
                var main = particleSystem.main;
                main.stopAction = ParticleSystemStopAction.Callback;

                var lifetime = effect.GetComponent<PooledEffect>();

                if (lifetime == null)
                {
                    lifetime = effect.gameObject.AddComponent<PooledEffect>();
                }

                lifetime.Initialize(
                    EffectsManager.Instance,
                    _prefabInstanceId,
                    particleSystem);

                return effect;
            }

            private static void OnGet(CFXR_Effect effect)
            {
                // Подготовка выполняется в EffectsManager.PrepareEffect.
            }

            private void OnRelease(CFXR_Effect effect)
            {
                if (effect == null)
                {
                    return;
                }

                var lifetime = effect.GetComponent<PooledEffect>();
                lifetime.Disarm();

                var particleSystem = effect.GetComponent<ParticleSystem>();

                particleSystem.Stop(
                    withChildren: true,
                    stopBehavior: ParticleSystemStopBehavior.StopEmittingAndClear);

                particleSystem.Clear(withChildren: true);

                effect.ResetState();

                effect.transform.SetParent(_poolRoot, worldPositionStays: false);
                effect.gameObject.SetActive(false);
            }

            private static void OnDestroy(CFXR_Effect effect)
            {
                if (effect != null)
                {
                    Destroy(effect.gameObject);
                }
            }
        }
    }
}