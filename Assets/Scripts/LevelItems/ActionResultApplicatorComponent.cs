using System;
using System.Collections.Generic;
using Arcatech.Actions;
using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Assertions;


namespace Arcatech.Triggers
{

    [RequireComponent(typeof(TriggerTrackerComponent),typeof(BaseGameEntityComponent))]
    public class ActionResultApplicatorComponent : ValidatedMonoBehaviour,IKillableComponent, IPausableComponent,ITriggerNotificationReceiver
    {
        [Header("Action result applicator")]
        [SerializeField] protected TargetingType targetType;
        [Header("if 0, apply once. if >0, apply the results every f seconds")]
        [SerializeField, Range(0,3)] protected float ReapplyWhileActorInsideTimer = 0;
        [Space]
        [Space, SerializeField] protected SerializedActionResult[] ResultOnEntry;
        [SerializeField] protected bool DestroyOnEnter = false;
        [SerializeField] protected SerializedActionResult[] ResultOnExit;
        [SerializeField] protected bool DestroyOnExit = false;

        Timer reapplyTimer;
        [SerializeField,Self] BaseGameEntityComponent baseComp;
        [SerializeField,Self] TriggerTrackerComponent triggerTracker;

        private List<IKillableComponent> killables = new();
        
        
        protected override void OnValidate()
        {
            base.OnValidate();
            Assert.IsFalse(targetType == TargetingType.None || targetType == TargetingType.OnlyUser,$"Incorrect targeting type set for {this}");
        }

        private void Start()
        {
            killables = new List<IKillableComponent>(GetComponentsInChildren<IKillableComponent>());
        }

        private void Update()
        {
            if (Killed||Paused) return;
            if (reapplyTimer is { IsRunning: true })
            {
                reapplyTimer.Tick(Time.deltaTime);
                if (!reapplyTimer.IsRunning)
                {
                    triggerTracker.RecheckCollisions();
                    reapplyTimer.Start();
                }
            }
        }

        public void TriggerEntered(BaseGameEntityComponent enterComponent, BaseGameEntityComponent trigger)
        {
            if (Killed || Paused) return;
            CheckTarget(enterComponent);
                if (DestroyOnEnter)
                {
                    foreach (var killable in killables)
                    {
                        killable.Killed = true;
                        return;
                    }
                }
                
                
                if (reapplyTimer == null)
                {
                    reapplyTimer = new CountDownTimer(ReapplyWhileActorInsideTimer);                    
                }
                reapplyTimer.Start();
            
        }

        public void TriggerExited(BaseGameEntityComponent exitComponent, BaseGameEntityComponent trigger)
        {
            if (Killed || Paused) return;
            CheckTarget(exitComponent);
            if (DestroyOnExit)
            {
                foreach (var killable in killables)
                {
                    killable.Killed = true;
                    return;
                }
            }
        }


        void CheckTarget(BaseGameEntityComponent enterComponent)
        {
            switch (targetType)
            {
                case TargetingType.AnyUnit:
                    ApplyResultsTo(enterComponent,ResultOnEntry);
                    break;
                case TargetingType.AnyEnemy:
                    if (enterComponent.GetEntitySide != baseComp.GetEntitySide) ApplyResultsTo(enterComponent, ResultOnEntry);
                    break;
                case TargetingType.AnyAlly:
                    if (enterComponent.GetEntitySide == baseComp.GetEntitySide) ApplyResultsTo(enterComponent, ResultOnEntry);
                    break;
                default:
                    Debug.Log($"{enterComponent.GetName} entered {this} and nothing happened because of trigger settings");
                    break;
            }
        }
        
        void ApplyResultsTo(BaseGameEntityComponent p, SerializedActionResult[] results)
        {
            foreach (var action in results)
            {
                action.BuildActionResult().ProduceResult(null, p, transform);
            }
        }

        private bool k = false;
        private bool p = false;

        public bool Killed { get => k;
            set
            {
                k = value;
                Debug.Log($"Killed {this}");
            }
        }
        public bool Paused { get => p; set { p = value; Debug.Log($"Paused {this}"); } }
    }
}