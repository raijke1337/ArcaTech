
using System;
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
using UnityEngine.Serialization;

namespace Arcatech
{
    /// <summary>
    /// new component that defines any game entity
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(Collider), typeof(LittlePauseHelperComponent))]
    public class BaseGameEntityComponent : ValidatedMonoBehaviour, IKillableComponent, IPausableComponent, IInvulnerability
    {
        [SerializeField, Self] LittlePauseHelperComponent _pauser;
        /// <summary>
        /// use this to pause.
        /// </summary>
        public LittlePauseHelperComponent Pauser => _pauser;
        [SerializeField, Self] Rigidbody _rb;
        [SerializeField] private bool setKinematic = true;
        
        [Space, SerializeField] string _name;
        [SerializeField] Side entitySide;
        [SerializeField] Transform effectSpawn;
        [SerializeField] private bool destroyOnDeath = true;
        [SerializeField, Range(0, 10)] private float timerToDestroy = 2f;
        
        [Space, SerializeField] protected bool _showDebugs = false;
        public Transform EffectSpawn => effectSpawn;
        
        List<IAppliedEffectsTakerComponent<AppliedStatsDeltaEffect>> _effectsTakerComponents;

        
        public string GetName =>  _name;
        public Side GetEntitySide => entitySide;
        public bool ShowingDebugs => _showDebugs;
        private Collider Collider { get; set; }


        protected override void OnValidate()
        {
            base.OnValidate();
            gameObject.layer = LayerMask.NameToLayer("Entities");
            if (effectSpawn == null)
            {
                effectSpawn = transform;
            }
        }

        private void OnEnable()
        {
            Collider = GetComponent<Collider>();
            _rb.isKinematic = setKinematic;
            _effectsTakerComponents = new List<IAppliedEffectsTakerComponent<AppliedStatsDeltaEffect>>(GetComponentsInChildren<IAppliedEffectsTakerComponent<AppliedStatsDeltaEffect>>());
        }

        
        #region applied usable effects

        public bool ApplyStatsEffect(BaseAppliedEffect effect,BaseGameEntityComponent source)
        {
            if (Invulnerable) return false;
            if (effect is AppliedStatsDeltaEffect statsDelta)
            {
                foreach (var v in _effectsTakerComponents)
                {
                    v.ApplyEffect(statsDelta, source);
                }
            }
            return true;
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
}