using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Stats
{
    /// <summary>
    /// moved the augment logic to this for clarity
    /// </summary>
    [RequireComponent(typeof(EntityStatsComponent),typeof(EntityStateMachineComponent))]
    public class StatsStateAugmentorComponent : ValidatedMonoBehaviour, IStateAugmentor, IKillerComponent
    {
        
        [SerializeField, Self]
        private EntityStatsComponent stats;
        [SerializeField,Self] EntityStateMachineComponent stateMachine;
        [SerializeField] private SerializedStateTransition toKilledState;
        [SerializeField] private SerializedStateTransition toKnockDown;
        private StateTransition _toKilled;
        private StateTransition _toKnockDown;
        private UnitState _killState;
        private UnitState _knockDownStart;
        
        private StateMachineContext _stateMachineCtx;

        public void Attach(IStateAugmentorReceiver machine)
        {
            if (toKilledState != null)
            {
                _toKilled = toKilledState.Build();
                machine.AddTransition(_toKilled);
                
                
                _killState = _toKilled.NextState;
            }

            if (toKnockDown != null)
            {
                _toKnockDown = toKnockDown.Build();
                machine.AddTransition(_toKnockDown);
                
                
                _knockDownStart = _toKnockDown.NextState;
            }
            _stateMachineCtx = machine.Context; // TODO: set some trigger for stagger / dmg anim instead of plain stat condition
        }

        public void Detach(IStateAugmentorReceiver machine)
        {
            if (_toKilled != null) machine.RemoveTransition(_toKilled);
            if (_toKnockDown != null) machine.RemoveTransition(_toKnockDown);
        }

        public void OnStateEntered(UnitState state, StateMachineContext context)
        {
            if (state == _knockDownStart)
            {
                context.KnockDownState = true;
            }

            if (state == _killState)
            {
                context.DeadState = true;
            }

            if (state.StateName == "KnockDownEnd")
            {
                context.KnockDownState = false;
            }
        }

        public void OnStateExited(UnitState state, StateMachineContext context)
        {

            if (state == _killState)
            {
                var killables = GetComponentsInChildren<IKillableComponent>(true);
                foreach (var k in killables)
                {
                    k.SetKilled(this,true);
                }
            }
        }

        public string KilledBy => "Entered DeadState in StateMachine";

    }
}