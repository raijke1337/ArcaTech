using System;
using System.Collections.Generic;
using System.Linq;
using Arcatech.Actions;
using Arcatech.Units;
using Arcatech.Usables.Effects;
using KBCore.Refs;
using Unity.U2D.Physics;
using UnityEngine;

namespace Arcatech.Stats
{
    /// <summary>
    /// moved the augment logic to this for clarity
    /// </summary>
    [RequireComponent(typeof(EntityStatsComponent),typeof(EntityStateMachineComponent))]
    public class StatsStateAugmentorComponent : ValidatedMonoBehaviour, IStateAugmentor, IKillerComponent,IStatUpdatesViewer
    {
        private static readonly int DamageDirectionHash = Animator.StringToHash("DamageDirection");
        private static readonly int DamageMagnitudeHash = Animator.StringToHash("DamageMagnitude");
        private const float MediumDamageFraction = 0.1f;
        private const float HeavyDamageFraction = 0.25f;
        
        [SerializeField, Self]
        private EntityStatsComponent stats;
        [SerializeField,Self] EntityStateMachineComponent stateMachine;
        [SerializeField] private SerializedStateTransition toKilledState;
        [SerializeField] private SerializedStateTransition toKnockDown;
        [SerializeField] private SerializedStateTransition toDamageInterrupt;
        [SerializeField] private bool setDamageAnimationParameters;

        [SerializeField] private SerializedActionResult onDeathPlaceholder;
        
        private StateTransition _toKilled;
        private StateTransition _toKnockDown;
        private StateTransition _toDamageInterrupt;

        private UnitState _killState;
        private UnitState _knockDownStartState;
        private UnitState _damageInterruptState;
        private float _queuedDamageDirection;
        private float _queuedDamageMagnitude;

        
        private StateMachineContext _stateMachineCtx;
        private List <IKillableComponent> _components;

        private bool usesKilled;
        private bool usesKnockDown;
        private bool usesDamageInterrupt;
        
        
        bool initialized = false;
        
        private void Start()
        {
            if (initialized) return;
            initialized = true;
            
            _components = GetComponentsInChildren<IKillableComponent>().ToList();
            usesKilled = toKilledState;
            usesKnockDown = toKnockDown;
            usesDamageInterrupt = toDamageInterrupt;
            

        }

        public void Attach(IStateAugmentorReceiver machine)
        {
            if (!initialized) Start();
            
            if (usesKilled)
            {
                _toKilled = toKilledState.Build();
                machine.AddTransition(_toKilled);
                _killState = _toKilled.NextState;
            }

            if (usesKnockDown)
            {
                _toKnockDown = toKnockDown.Build();
                machine.AddTransition(_toKnockDown);
                _knockDownStartState = _toKnockDown.NextState;
            }
            
            if (usesDamageInterrupt)
            {
                _toDamageInterrupt??= toDamageInterrupt.Build();
                _damageInterruptState = _toDamageInterrupt.NextState;
                machine.AddTransition(_toDamageInterrupt);
            }
            _stateMachineCtx = machine.Context;
            stats.RegisterStatsViewer(this);
        }

        public void Detach(IStateAugmentorReceiver machine)
        {
            if (usesKilled) machine.RemoveTransition(_toKilled);
            if (usesKnockDown) machine.RemoveTransition(_toKnockDown);
            if (usesDamageInterrupt) machine.RemoveTransition(_toDamageInterrupt);
            stats.UnregisterStatsViewer(this);
            _stateMachineCtx = null;
        }

        public void OnStateEntered(UnitState state, StateMachineContext context)
        {
             if (usesKnockDown && state == _knockDownStartState)
             {
                 context.StunnedState = true;
             }

            if (usesKilled && state == _killState)
            {
                foreach (var c in _components)
                {
                    c.SetKilled(this,true);
                    onDeathPlaceholder?.Deserialize().ProduceResult(stateMachine.GetMainEntity,stateMachine.GetMainEntity,stateMachine.GetMainEntity.EffectSpawn.position,stateMachine.GetMainEntity.EffectSpawn.rotation);
                }
                context.DeadState = true;
            }

            if (usesKnockDown && state.StateName == "KnockDownEnd")
            {
                context.StunnedState = false;
            }

            if (usesDamageInterrupt && state == _damageInterruptState)
            {
                if (setDamageAnimationParameters && context.Animator != null)
                {
                    context.Animator.SetFloat(DamageDirectionHash, _queuedDamageDirection);
                    context.Animator.SetFloat(DamageMagnitudeHash, _queuedDamageMagnitude);
                }

                context.InterruptQueued = false;
            }
        }

        public void OnStateExited(UnitState state, StateMachineContext context)
        {
            if (usesKilled && state == _killState)
            {
                var killables = GetComponentsInChildren<IKillableComponent>(true);
                foreach (var k in killables)
                {
                    k.SetKilled(this,true);
                }
            }
        }

        public string KilledBy => $"Transition to Killed State Condition Satisfied";


        public void HandleStatsUpdate(ResourceStatType stat, float statCurrent, float statMax, float statDelta, EntityStatsComponent.ExpendType changeType,
            BaseGameEntityComponent source)
        {
            if (_stateMachineCtx != null && stat == ResourceStatType.Health &&
                statDelta < 0f &&
                changeType == EntityStatsComponent.ExpendType.ActionResult &&
                source != stateMachine.GetMainEntity)
            {
                if (setDamageAnimationParameters)
                {
                    // Keep the active reaction's blend values until the next interrupt starts.
                    var facing = stateMachine.GetMainEntity.transform;
                    _queuedDamageDirection = 1f;
                    if (source != null)
                    {
                        Vector3 toSource = Vector3.ProjectOnPlane(source.transform.position - facing.position, Vector3.up);
                        if (Vector3.Dot(facing.forward, toSource) < 0f)
                            _queuedDamageDirection = -1f;
                    }

                    float damageFraction = -statDelta / statMax;
                    _queuedDamageMagnitude = damageFraction >= HeavyDamageFraction ? 3f :
                        damageFraction >= MediumDamageFraction ? 2f : 1f;
                }

                _stateMachineCtx.InterruptQueued = true;
            }
        }

        public void SetShieldValue(ResourceStatType shieldStat, float currentValue)
        { }
    }

}
