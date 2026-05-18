using System.Collections.Generic;
using Arcatech.Stats;
using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech
{
    /// <summary>
    /// new component that defines any game entity
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(LittlePauseHelperComponent))]
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
        public UnityEvent<BaseGameEntityComponent> AnnounceDead;

        public void SetKilled(IKillerComponent comp, bool value)
        {
            if (ShowingDebugs) Debug.Log($"{GetName} {(value ? "dead" : "resurrected")}, called by: {comp.KilledBy}");
            _killed = value;
            AnnounceDead?.Invoke(this);
            if (_killed && destroyOnDeath)
            {
                Destroy(gameObject, timerToDestroy);
            }
        }
        public bool Paused { get; set; }

    }
}