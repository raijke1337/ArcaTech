using CartoonFX;
using UnityEngine;

namespace Arcatech.Managers
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ParticleSystem))]
    public sealed class PooledEffect : MonoBehaviour
    {
        public EntityId PrefabInstanceId { get; private set; }

        private EffectsManager _manager;
        private CFXR_Effect _effect;
        private ParticleSystem _rootParticleSystem;

        private bool _isArmed;
        private bool _rootSystemStopped;

        public void Initialize(
            EffectsManager manager,
            EntityId prefabInstanceId,
            ParticleSystem rootParticleSystem)
        {
            _manager = manager;
            PrefabInstanceId = prefabInstanceId;
            _rootParticleSystem = rootParticleSystem;
            _effect = GetComponent<CFXR_Effect>();
        }

        public void Arm()
        {
            _rootSystemStopped = false;
            _isArmed = true;
        }

        public void Disarm()
        {
            _isArmed = false;
            _rootSystemStopped = false;
        }

        private void OnParticleSystemStopped()
        {
            if (!_isArmed)
            {
                return;
            }

            _rootSystemStopped = true;
        }

        private void Update()
        {
            if (!_isArmed || !_rootSystemStopped)
            {
                return;
            }

            // Включая дочерние particle systems и sub-emitter'ы.
            if (_rootParticleSystem.IsAlive(withChildren: true))
            {
                return;
            }

            _isArmed = false;
            _manager.Release(_effect);
        }
    }
}