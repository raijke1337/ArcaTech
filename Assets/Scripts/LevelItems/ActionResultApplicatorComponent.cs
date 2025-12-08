using System;
using System.Collections.Generic;
using System.Linq;
using Arcatech.Actions;
using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Assertions;


namespace Arcatech.Triggers
{

    [RequireComponent(typeof(TriggerTrackerComponent), typeof(BaseGameEntityComponent))]
    public class ActionResultApplicatorComponent : ValidatedMonoBehaviour, IKillableComponent, IPausableComponent,
        ITriggerNotificationReceiver
    {
        [SerializeField, Self] BaseGameEntityComponent baseComp;
        [SerializeField, Self] TriggerTrackerComponent triggerTracker;


        [SerializeField,
         Tooltip("If enabled, will apply to any unit entering. Still checks the application logic in results")]
        private bool applyToAllTargets = false;


        [Header("if 0, apply once. if >0, apply the results every f seconds")] [SerializeField, Range(0, 3)]
        protected float ReapplyWhileActorInsideTimer = 0;

        [SerializeField] protected SerializedActionResult[] resultOnExit;
        [Space, SerializeField] protected SerializedActionResult[] resultOnEntry;
        [Space] [SerializeField] protected bool killEntityOnEnter = false;
        [SerializeField] protected bool killEntityOnExit = false;

        private List<IKillableComponent> killables = new();

        private ActionResult[] _entry;
        private ActionResult[] _exit;

        Timer reapplyTimer;


        private void Start()
        {
            _entry = resultOnEntry?.Length > 0
                ? resultOnEntry.Select(t => t.Deserialize()).ToArray()
                : Array.Empty<ActionResult>();
            _exit = resultOnExit?.Length > 0
                ? resultOnExit.Select(t => t.Deserialize()).ToArray()
                : Array.Empty<ActionResult>();

            killables = new List<IKillableComponent>(GetComponentsInChildren<IKillableComponent>());
        }

        private void Update()
        {
            if (Killed || Paused) return;
            if (reapplyTimer is { IsRunning: true })
            {
                reapplyTimer.Tick(Time.deltaTime);
                if (!reapplyTimer.IsRunning)
                {
                    triggerTracker.Active = false;
                    reapplyTimer.Start();
                    triggerTracker.Active = true;
                }
            }
        }

        public void TriggerEntered(TriggerHitInfo info)
        {
            if (Killed || Paused) return;
            if (!info.IsValidHit || info.Target == baseComp) return;

            if (info.Target.CompareTag("Player") || applyToAllTargets)
            {
                foreach (var action in _entry)
                {
                    action.ProduceResult(baseComp, info.Target, baseComp.EffectSpawn.position,
                        baseComp.EffectSpawn.rotation);
                }
            }

            if (killEntityOnEnter)
            {
                foreach (var killable in killables)
                {
                    killable.Killed = true;
                    return;
                }
            }
            reapplyTimer ??= new CountDownTimer(ReapplyWhileActorInsideTimer);
            reapplyTimer.Start();
        }
    

    public void TriggerExited(TriggerHitInfo triggerExitInfo)
        {
            if (Killed || Paused || !triggerExitInfo.IsValidHit ) return;
            
            if (triggerExitInfo.Target.CompareTag("Player") || applyToAllTargets)
            {
                foreach (var action in _entry)
                {
                    action.ProduceResult(baseComp, triggerExitInfo.Target, baseComp.EffectSpawn.position,
                        baseComp.EffectSpawn.rotation);
                }
            }
            if (killEntityOnExit)
            {
                foreach (var killable in killables)
                {
                    killable.Killed = true;
                    return;
                }
            }
        }


        public bool Killed { get; set; } = false;

        public bool Paused { get; set; } = false;
    }
}