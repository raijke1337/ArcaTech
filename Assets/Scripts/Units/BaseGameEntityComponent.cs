
using System.Collections.Generic;
using System.Linq;
using Arcatech.EventBus;
using Arcatech.Interactions;
using Arcatech.Managers;
using Arcatech.Stats;
using Arcatech.Triggers;
using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Arcatech
{
    /// <summary>
    /// new component that defines any game entity
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(Collider), typeof(LittlePauseHelperComponent))]
    public class BaseGameEntityComponent : ValidatedMonoBehaviour, IKillableComponent, IPausableComponent, ISpawnerProvider,IInvulnerability
    {
        [SerializeField, Self] LittlePauseHelperComponent _pauser;

        [Space, SerializeField] string _name;
        [SerializeField] Side entitySide;
        [SerializeField] Transform _effectSpawn;
        [SerializeField] private bool destroyOnDeath = true;
        [SerializeField, Range(0, 10)] private float timerToDestroy = 2f;
        
        [Space, SerializeField] protected bool _showDebugs = false;
        public Transform EffectSpawn => _effectSpawn;

        List<IAppliedEffectsTakerComponent<AppliedStatsDeltaEffect>> _effectsTakerComponents;
        #if UNITY_EDITOR
        public IReadOnlyList<IAppliedEffectsTakerComponent<AppliedStatsDeltaEffect>> GetEffectsTakersForEditor=> _effectsTakerComponents;
        #endif
        
        public string GetName =>  _name;
        public Side GetEntitySide => entitySide;
        public bool ShowingDebugs => _showDebugs;
        private Collider Collider { get; set; }


        protected override void OnValidate()
        {
            base.OnValidate();
            gameObject.layer = LayerMask.NameToLayer("Entities");
            if (_effectSpawn == null)
            {
                _effectSpawn = transform;
            }
        }

        private void OnEnable()
        {
            Collider = GetComponent<Collider>();
            
            _effectsTakerComponents = new List<IAppliedEffectsTakerComponent<AppliedStatsDeltaEffect>>(GetComponentsInChildren<IAppliedEffectsTakerComponent<AppliedStatsDeltaEffect>>());
        }

        
        #region stats effects



        public void ApplyStatsEffect(AppliedStatsDeltaEffect effect,BaseGameEntityComponent source)
        {
            if (Invulnerable) return;
            foreach (var v in _effectsTakerComponents)
            {
                v.ApplyEffect(effect, source);
            }
        }

        #endregion
        
        public bool Invulnerable { get; set; }
        public bool EntityAlive => !_killed;
        bool _killed = false;
        public void SetKilled(IKillerComponent comp, bool value)
        {
            if (ShowingDebugs) Debug.Log($"{GetName} dead, called by: {comp.KilledBy}");
            
            _killed = value;
            //    _rigidbody.isKinematic = !value;
            Collider.isTrigger = value;
            if (_killed && destroyOnDeath)
            {
                Destroy(gameObject, timerToDestroy);
            }
        }

        public bool Paused { get; set; }

    }

    public interface IInvulnerability
    {
        public bool Invulnerable { get; set; }
    }
}