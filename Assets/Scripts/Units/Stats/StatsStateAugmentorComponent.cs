using System;
using System.Collections.Generic;
using System.Linq;
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
        
        [SerializeField, Self]
        private EntityStatsComponent stats;
        [SerializeField,Self] EntityStateMachineComponent stateMachine;
        [SerializeField] private SerializedStateTransition toKilledState;
        [SerializeField] private SerializedStateTransition toKnockDown;
        [SerializeField] private SerializedStateTransition toDamageInterrupt;

        
        private StateTransition _toKilled;
        private StateTransition _toKnockDown;
        private StateTransition _toDamageInterrupt;

        private UnitState _killState;
        private UnitState _knockDownStartState;
        private UnitState _damageInterruptState;

        
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
            stats.RegisterStatsViewer(this);
            _stateMachineCtx = machine.Context;
        }

        public void Detach(IStateAugmentorReceiver machine)
        {
            if (usesKilled) machine.RemoveTransition(_toKilled);
            if (usesKnockDown) machine.RemoveTransition(_toKnockDown);
            if (usesDamageInterrupt) machine.RemoveTransition(_toDamageInterrupt);
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
                }
                context.DeadState = true;
            }

            if (usesKnockDown && state.StateName == "KnockDownEnd")
            {
                context.StunnedState = false;
            }

            if (usesDamageInterrupt && state == _damageInterruptState)
            {
                _stateMachineCtx.InterruptQueued = false;
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
            // if (state == _damage)
            // {
            //     context.Animator.SetFloat(dmgFrontHash,0);
            //     context.Animator.SetFloat(dmgRightHash,0);
            // }
        }

        public string KilledBy => $"Transition to Killed State Condition Satisfied";


        public void HandleStatsUpdate(ResourceStatType stat, float statCurrent, float statMax, float statDelta, EntityStatsComponent.ExpendType changeType,
            BaseGameEntityComponent source)
        {
            if (stat == ResourceStatType.Health &&
                statDelta < 0f &&
                changeType == EntityStatsComponent.ExpendType.ActionResult &&
                source != stateMachine.GetMainEntity)
            {
                // damage was taken
                _stateMachineCtx.InterruptQueued = true;
            }
        }

        public void SetShieldValue(ResourceStatType shieldStat, float currentValue)
        { }
    }

}