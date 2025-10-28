
using System.Collections.Generic;
using Arcatech.EventBus;
using Arcatech.Triggers;
using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;
namespace Arcatech
{
    /// <summary>
    /// new component that defines any game entity
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(Collider), typeof(LittlePauseHelperComponent))]
    public class BaseGameEntityComponent : ValidatedMonoBehaviour, IKillableComponent, IPausableComponent
    {
        [SerializeField, Self] LittlePauseHelperComponent _pauser;

        [Space, SerializeField] string _name;
        [SerializeField] Side entitySide;
        [SerializeField] Transform _spawnPoint;
        [SerializeField] private bool destroyOnDeath = false;
        [SerializeField, Range(0.1f, 10)] private float timerToDestroy = 2f;
        
        [Space, SerializeField] protected bool _showDebugs = false;
        public Transform SpawnPoint => _spawnPoint;


        List<IEffectsTakerComponent> _effectsTakerComponents;
        #if UNITY_EDITOR
        public IReadOnlyList<IEffectsTakerComponent> GetEffectsTakersForEditor=> _effectsTakerComponents;
        #endif
        
        public string GetName =>  _name;
        public Side GetEntitySide => entitySide;
        public bool ShowingDebugs => _showDebugs;
        private Collider Collider { get; set; }

        [Space, Header("Rigidbody override"), SerializeField, Self]
        Rigidbody _rigidbody;

        [SerializeField] bool gravity = false;
        [SerializeField] bool usePhysics = false;


        protected override void OnValidate()
        {
            base.OnValidate();
            gameObject.layer = LayerMask.NameToLayer("Entities");
            if (_spawnPoint == null)
            {
                _spawnPoint = transform;
            }
        }

        private void OnEnable()
        {
            Collider = GetComponent<Collider>();
            _rigidbody.useGravity = gravity;
            _rigidbody.isKinematic = !usePhysics;
            _effectsTakerComponents = new List<IEffectsTakerComponent>(GetComponentsInChildren<IEffectsTakerComponent>());
            
        }

        public void ApplyStatsEffect(StatsEffect effect,BaseGameEntityComponent source)
        {
            if (_showDebugs) Debug.Log($"ApplyStatsEffect {effect}: {this}");
            foreach (var handler in _effectsTakerComponents)
            {
                handler.ApplyEffect(effect,source);
            }
        }
        
        bool _killed = false;

        public bool Killed
        {
            get => _killed;
            set
            {
                _killed = value;
                _rigidbody.isKinematic = !value;
                Collider.isTrigger = value;
                if (_killed && destroyOnDeath)
                {
                    Destroy(gameObject, timerToDestroy);
                }
            }
        }

        public bool Paused { get; set; }
    }
}