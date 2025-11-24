using System;
using Arcatech.Actions;
using Arcatech.Skills;
using Arcatech.Triggers;
using Arcatech.Units;
using System.Linq;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Items.Projectiles
{
    [RequireComponent(typeof(TriggerTrackerComponent), typeof(BaseGameEntityComponent))]
    public class ProjectileComponent : ValidatedMonoBehaviour, IPausableComponent, ITriggerNotificationReceiver
    {

        [SerializeField, Self] private BaseGameEntityComponent entity;
        
        [HideInInspector] public int RemainingHits;
        [HideInInspector] public float Lifetime;
        [HideInInspector] public float Speed;
        
        TriggerTrackerComponent col;

        ProjectileBehavior _behavior;
        
        private void Start()
        {
            col = GetComponent<TriggerTrackerComponent>();
            col.RegisterReceiver(this);
        }

        public void Setup(BaseGameEntityComponent owner, SerializedProjectileBehavior behavior)
        {
            _behavior = behavior.Deserialize();
        }
        
        private void OnDestroy()
        {
            col.UnregisterReceiver(this);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.isStatic)
            {
                Debug.Log("Collision Enter static item, should destroy");
                RemainingHits = 0;
            }
        }

        public void TriggerEntered(TriggerHitInfo triggerHitInfo)
        {

        }

        public void TriggerExited(BaseGameEntityComponent exitComponent, ITriggerNotificationProvider trigger)
        {
            //NOOP
        }
        
        protected virtual void Update()
        {
            if (Paused) return;

            _behavior.UpdatePosition(Time.deltaTime,transform);
            if (_behavior.IsExpired) Expiry();
        }

        private void Expiry()
        {
            
        }
        
        public bool Paused { get; set; } = false;

    }
}
