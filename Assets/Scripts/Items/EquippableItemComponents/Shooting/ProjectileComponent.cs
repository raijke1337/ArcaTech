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
    public class ProjectileComponent : ValidatedMonoBehaviour, IPausableComponent, ITriggerNotificationReceiver,ITriggerNotificationProvider
    {

        #region projectilEvents
        public event UnityAction<ProjectileComponent> ProjectileExpiredEvent = delegate { };
       
        
        #endregion
        
        [SerializeField, Self] private BaseGameEntityComponent entity;
        public BaseGameEntityComponent Entity => entity;
        
        
        TriggerTrackerComponent col;
        ProjectileBehavior _behavior;
        
        ITriggerNotificationReceiver _receiver;
        
        private void Start()
        {
            col = GetComponent<TriggerTrackerComponent>();
            col.RegisterReceiver(this);
        }

        public void Setup(BaseGameEntityComponent owner, SerializedProjectileBehavior behavior)
        {
            _behavior = behavior.Deserialize();
        }

        public void TriggerEntered(TriggerHitInfo triggerHitInfo) => _receiver?.TriggerEntered(triggerHitInfo);
        public void TriggerExited(BaseGameEntityComponent exitComponent, ITriggerNotificationProvider trigger) => _receiver?.TriggerExited(exitComponent, trigger);
        protected virtual void Update()
        {
            if (Paused) return;

            _behavior.UpdatePosition(Time.deltaTime,transform);
            if (_behavior.IsExpired) ProjectileExpiredEvent.Invoke(this);
        }
        public bool Paused { get; set; } = false;

        public void Reset()
        {
            _behavior.Reset();
        }
        public bool Active { get; set; }
        public void RegisterReceiver(ITriggerNotificationReceiver receiver)=> _receiver = receiver;
        public void UnregisterReceiver(ITriggerNotificationReceiver receiver) => _receiver = null;
        public int LayerMaskIndex { get; set; }
    }
}
