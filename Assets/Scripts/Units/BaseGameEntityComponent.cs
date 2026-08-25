using System.Collections.Generic;
using Arcatech.SaveSystem;
using Arcatech.Stats;
using Arcatech.Units;
using Arcatech.Usables.Effects;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech
{
    /// <summary>
    /// new component that defines any game entity
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(LittlePauseHelperComponent),typeof(EntityID))]
    public class BaseGameEntityComponent : ValidatedMonoBehaviour, IKillableComponent, IPausableComponent, IInvulnerability
    {
        
        [SerializeField, Self] LittlePauseHelperComponent _pauser;
       
        [Space, SerializeField] string _name;
        [SerializeField, Self] private EntityID id;
        
        
        
        [SerializeField, Self] Rigidbody _rb;
        [SerializeField] private bool setKinematic = true;
        

        [SerializeField] Side entitySide;
        [SerializeField] Transform effectSpawn;
        [SerializeField] private bool destroyOnDeath = true;
        [SerializeField, Range(0, 10)] private float timerToDestroy = 2f;
        
        [Space, SerializeField] protected bool _showDebugs = false;
        public Transform EffectSpawn => effectSpawn;
        [SerializeField] private bool targetable = false;
        public bool Targetable => targetable;
        
        /// <summary>
        /// use this to pause.
        /// </summary>
        public LittlePauseHelperComponent Pauser => _pauser;

        public string GetID => id.UniqueId;
        public string GetName =>  string.IsNullOrEmpty(_name) ? gameObject.name : _name;
        public Side GetEntitySide => entitySide;
        public bool ShowingDebugs => _showDebugs;
        public void AssignName(string n) => _name = n;

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
        }
        
        public bool Invulnerable { get; set; }
        
        public bool EntityAlive => !_killed;
        bool _killed = false;
        public UnityEvent<BaseGameEntityComponent> AnnounceDead;

        public bool Paused { get; set; }
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
    }
}