using System;
using Arcatech.Actions;
using Arcatech.Skills;
using Arcatech.Triggers;
using Arcatech.Units;
using System.Linq;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Items.Projectiles
{
    [RequireComponent(typeof(TriggerTrackerComponent), typeof(BaseGameEntityComponent))]
    public sealed class ProjectileComponent : ValidatedMonoBehaviour, IPausableComponent, ITriggerNotificationReceiver,ITriggerNotificationProvider
    {

        #region projectilEvents
        public event UnityAction<ProjectileComponent> ProjectileFinished = delegate { };
       
        
        #endregion
        
        [SerializeField, Self] private BaseGameEntityComponent entity;
        public BaseGameEntityComponent Entity => entity;
        
        TriggerTrackerComponent col;
        ProjectileBehavior _behavior;
        private int _maxHits;
        int _currentHits = 0;
        private BaseGameEntityComponent _owner;
        
        ITriggerNotificationReceiver _receiver;
        
        private void Start()
        {
            col = GetComponent<TriggerTrackerComponent>();
            col.RegisterReceiver(this);
        }

        public void Setup(BaseGameEntityComponent owner, SerializedProjectileBehavior behavior, int maxHits)
        {
            _behavior = behavior.Deserialize(owner);
            _maxHits = maxHits;
            _owner = owner;
        }

        public void TriggerEntered(TriggerHitInfo triggerHitInfo)
        {
            _receiver?.TriggerEntered(triggerHitInfo);
            if (!triggerHitInfo.IsValidHit || triggerHitInfo.Target== _owner) return;
            _currentHits++;
            _behavior.NotifyCollision(triggerHitInfo);
        }
        public void TriggerExited(TriggerHitInfo triggerExitInfo) => _receiver?.TriggerExited(triggerExitInfo);
        void Update()
        {
            if (Paused) return;

            _behavior.UpdatePosition(Time.deltaTime,transform);
            if (_behavior.BehaviorCompleted || _currentHits >= _maxHits) ProjectileFinished.Invoke(this);
        }
        public bool Paused { get; set; } = false;

        public void Reset()
        {
            _behavior.Reset();
            _currentHits = 0;
        }
        /// <summary>
        /// called by the spawner (hit producer)
        /// </summary>
        public bool Active { get; set; }
        public void RegisterReceiver(ITriggerNotificationReceiver receiver)=> _receiver = receiver;
        public void UnregisterReceiver(ITriggerNotificationReceiver receiver) => _receiver = null;

    }
}
