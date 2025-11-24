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
        [SerializeField,Self] BaseGameEntityComponent baseComp;
        [SerializeField,Self] TriggerTrackerComponent triggerTracker;
        [Header("Action result applicator")]
        [SerializeField] protected TargetingType targetType;
        [Header("if 0, apply once. if >0, apply the results every f seconds")]
        [SerializeField, Range(0,3)] protected float ReapplyWhileActorInsideTimer = 0;
        [SerializeField] protected SerializedActionResult[] resultOnExit;
        [Space, SerializeField] protected SerializedActionResult[] resultOnEntry;
        [Space]

        [SerializeField] protected bool killEntityOnEnter = false;

        [SerializeField] protected bool killEntityOnExit = false;
        private List<IKillableComponent> killables = new();
        
        
        
        Timer reapplyTimer;


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

        public void TriggerEntered(TriggerHitInfo info)
        {
            if (Killed || Paused) return;
            CheckTarget(info.Target,true);
                if (killEntityOnEnter)
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

        public void TriggerExited(BaseGameEntityComponent exitComponent, ITriggerNotificationProvider trigger)
        {
            if (Killed || Paused) return;
            CheckTarget(exitComponent,false);
            if (killEntityOnExit)
            {
                foreach (var killable in killables)
                {
                    killable.Killed = true;
                    return;
                }
            }
        }


        void CheckTarget(BaseGameEntityComponent enterComponent, bool entering)
        {
            switch (targetType)
            {
                case TargetingType.ApplyToEnemyTarget:
                    if (enterComponent.GetEntitySide!=baseComp.GetEntitySide) ApplyResultsTo(enterComponent,entering?resultOnEntry:resultOnExit);
                    break;
                case TargetingType.ApplyToAlliedTarget:
                    if (enterComponent.GetEntitySide==baseComp.GetEntitySide) ApplyResultsTo(enterComponent,entering?resultOnEntry:resultOnExit);
                    break;
                case TargetingType.ApplyToAnyTarget:
                    ApplyResultsTo(enterComponent,entering?resultOnEntry:resultOnExit);
                    break;
                default:
                    Debug.Log("Unknown target type");
                    break;
            }
        }
        
        void ApplyResultsTo(BaseGameEntityComponent p, SerializedActionResult[] results)
        {
            foreach (var action in results)
            {
//                Debug.Log($"Apply result {action} to {p.GetName}");
                action.BuildActionResult().ProduceResult(null, p, transform.position, transform.rotation);
            }
        }

        public bool Killed { get; set; } = false;

        public bool Paused { get; set; } = false;
    }
}