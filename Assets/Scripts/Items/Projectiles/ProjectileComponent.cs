using System;
using System.Collections.Generic;
using Arcatech.Triggers;
using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Items.Projectiles
{

    [RequireComponent(typeof(TriggerTrackerComponent), typeof(BaseGameEntityComponent))]
    public sealed class ProjectileComponent : ValidatedMonoBehaviour, IPausableComponent, ITriggerNotificationReceiver,
        ITriggerNotificationProvider
    {
        public event UnityAction<ProjectileComponent> ProjectileFinished = delegate { };

        [SerializeField, Self] private BaseGameEntityComponent entity;
        public BaseGameEntityComponent Entity => entity;

        ProjectileBehavior _behavior;
        TriggerTrackerComponent _col;
        
        private List<IEquipmentPart> _parts;
        ITriggerNotificationReceiver _receiver;


        private void Awake()
        {
            _col = GetComponent<TriggerTrackerComponent>();
            _parts =  new List<IEquipmentPart>();
            _parts.AddRange(GetComponentsInChildren<IEquipmentPart>());
        }

        private void OnEnable()
        {
            if (_col == null)
                _col = GetComponent<TriggerTrackerComponent>();

            _col.RegisterReceiver(this);
        }
        private void OnDisable()
        {
            if (_col != null)
                _col.UnregisterReceiver(this);
        }

        public void Setup(BaseGameEntityComponent owner, SerializedProjectileBehavior behavior)
        {
            _behavior = behavior.Deserialize(owner);
        }

        public void TriggerEntered(TriggerHitInfo triggerHitInfo)
        {
         //   Debug.Log($"{name} projectile trigger reporting hit {triggerHitInfo.TargetCollider.gameObject.name}");
            _receiver?.TriggerEntered(triggerHitInfo);
            _behavior.NotifyCollision(triggerHitInfo);
        }

        public void TriggerExited(TriggerHitInfo triggerExitInfo) => _receiver?.TriggerExited(triggerExitInfo);

        void Update()
        {
            if (Paused) return;
            _behavior.UpdatePosition(Time.deltaTime, transform);
            if (_behavior.BehaviorCompleted)
            {
                ProjectileFinished.Invoke(this);
            }
        }
    

         public bool Paused { get; set; } = false;

        public void Reset()
        {
            _behavior.Reset();
            transform.position = Vector3.zero;
        }

        /// <summary>
        /// called by the spawner (hit producer)
        /// </summary>
        public bool Active
        {
            get => _col.Active;
            set
            {
                _col.Active = value;
                //Debug.Log($"{this} {(value? "Activating" : "Deactivating")}");
                foreach (var part in _parts)
                {
                    part.TriggerState(value? StateMachineNotifyType.Use : StateMachineNotifyType.EndUse);
                }
            }
        }

        public void RegisterReceiver(ITriggerNotificationReceiver receiver)=> _receiver = receiver;
        public void UnregisterReceiver(ITriggerNotificationReceiver receiver) => _receiver = null;
        public void AreaCast(ITriggerNotificationReceiver receiver)
        {
            // noop
        }
        public void OnChangeUsableState(StateMachineNotifyType notification)
        {
            // this is not called in current implementation
        }
    }
}
