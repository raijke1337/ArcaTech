using System;
using System.Collections.Generic;
using System.Linq;
using Arcatech.Actions;
using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;


namespace Arcatech.Triggers
{
    [RequireComponent(typeof(TriggerTrackerComponent), typeof(BaseGameEntityComponent))]
    public class ActionResultApplicatorComponent : ValidatedMonoBehaviour, IKillableComponent, IPausableComponent,
        ITriggerNotificationReceiver, IKillerComponent
    {
        [SerializeField, Self] BaseGameEntityComponent baseComp;
        [SerializeField, Self] TriggerTrackerComponent triggerTracker;


        [SerializeField,
         Tooltip("If enabled, will activate on any unit entering. Still checks the application logic in results")]
        private bool applyToAllTargets = false;

        [Header("if 0, apply once. if >0, apply the results every f seconds")] [SerializeField, Range(0, 3)]
        protected float reapplyWhileActorInsideTimer = 0.5f;

        [Header("Activity timer")] [SerializeField]
        private bool useTimer = false;
        [SerializeField] private float activeTime = 2f;
        [SerializeField]  private float inactiveTime = 1.5f;
        [SerializeField] private Transform[] disableWhenInactive;
        private bool _isActiveTime = true;
        private float _phaseTime = 0f;
        [Space]

        [SerializeField] protected SerializedActionResult[] resultOnExit;
        [Space, SerializeField] protected SerializedActionResult[] resultOnEntry;
        [Space] [SerializeField] protected bool killEntityOnEnter = false;
        [SerializeField] protected bool killEntityOnExit = false;

        private List<IKillableComponent> killables = new();

        private ActionResult[] _entry;
        private ActionResult[] _exit;

        Timer _reapplyTimer;
        private bool _killed = false;


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
            if (_killed || Paused) return;

            if (useTimer)
            {
                _phaseTime += Time.deltaTime;
                if (_isActiveTime && _phaseTime >= activeTime)
                {
                    _isActiveTime = false;
                    _phaseTime = 0f;
                    if (disableWhenInactive.Length > 0)
                    {
                        foreach (var d in disableWhenInactive)
                        {
                            d.gameObject.SetActive(false);
                        }
                    }
                }

                if (!_isActiveTime && _phaseTime >= inactiveTime)
                {
                    _isActiveTime = true;
                    _phaseTime = 0f;
                    if (disableWhenInactive.Length > 0)
                    {
                        foreach (var d in disableWhenInactive)
                        {
                            d.gameObject.SetActive(true);
                        }
                    }
                }

                if (_isActiveTime)
                {
                    if (_reapplyTimer is { IsRunning: true })
                    {
                        _reapplyTimer.Tick(Time.deltaTime);
                        if (!_reapplyTimer.IsRunning)
                        {
                            triggerTracker.Active = false;
                            _reapplyTimer.Start();
                            triggerTracker.Active = true;
                        }
                    }
                }
            }

            if (!useTimer)
            {
                if (_reapplyTimer is { IsRunning: true })
                {
                    _reapplyTimer.Tick(Time.deltaTime);
                    if (!_reapplyTimer.IsRunning)
                    {
                        triggerTracker.Active = false;
                        _reapplyTimer.Start();
                        triggerTracker.Active = true;
                    }
                }
            }
        }

        public void TriggerEntered(TriggerHitInfo info)
        {
            if (_killed || Paused || !info.TargetCollider) return;
            if (!info.TargetCollider.TryGetComponent(out BaseGameEntityComponent entity)) return;
            if (entity == baseComp) return;

            if (entity.CompareTag("Player") || applyToAllTargets)
            {
                foreach (var action in _entry)
                {
                    action.ProduceResult(baseComp, entity, info.Position,
                        Quaternion.Euler(info.ImpactDirection));
                }
            }

            if (killEntityOnEnter)
            {
                foreach (var killable in killables)
                {
                    killable.SetKilled(this,true);
                    return;
                }
            }
            _reapplyTimer ??= new CountDownTimer(reapplyWhileActorInsideTimer);
            _reapplyTimer.Start();
        }
    

    public void TriggerExited(TriggerHitInfo info)
        {
            if (_killed || Paused || !info.TargetCollider) return;
            if (!info.TargetCollider.TryGetComponent(out BaseGameEntityComponent entity)) return;
            if (entity == baseComp) return;

            if (entity.CompareTag("Player") || applyToAllTargets)
            {
                foreach (var action in _exit)
                {
                    action.ProduceResult(baseComp, entity, info.Position,
                        Quaternion.Euler(info.ImpactDirection));
                }
            }

            if (!killEntityOnExit) return;
            foreach (var killable in killables)
            {
                killable.SetKilled(this,true);
                return;
            }
        }
        public bool Paused { get; set; } = false;
        public void SetKilled(IKillerComponent component, bool value)
        {
            _killed = value;
        }
        public string KilledBy => $"Action result applicator {name}";
    }
}