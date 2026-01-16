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
        [Header("Stats-related states apply only if a state machine is available")]
        [Header("--------------------------")]
        [SerializeField, Self]
        private EntityStatsComponent stats;
        [SerializeField,Self] EntityStateMachineComponent stateMachine;
        [SerializeField] private SerializedStateTransition toKilledState;
        [SerializeField] private SerializedStateTransition toStaggerState;
        private StateTransition _toKilled;
        private StateTransition _toStagger;
        private UnitState _killState;
        private UnitState _staggerState;
        
        private StateMachineContext _stateMachineCtx;

        public void Attach(IStateAugmentorReceiver machine)
        {
            if (toKilledState != null)
            {
                _toKilled = toKilledState.Build();
                _killState = _toKilled.NextState;
                machine.AddTransition(_toKilled);
            }

            if (toStaggerState != null)
            {
                _toStagger = toStaggerState.Build();
                _staggerState = _toStagger.NextState;
                machine.AddTransition(_toStagger);
            }
            _stateMachineCtx = machine.Context; // TODO: set some trigger for stagger / dmg anim instead of plain stat condition
        }

        public void Detach(IStateAugmentorReceiver machine)
        {
            if (_toKilled != null) machine.RemoveTransition(_toKilled);
            if (_toStagger != null) machine.RemoveTransition(_toStagger);
        }

        public void OnStateEntered(UnitState state, StateMachineContext context)
        {
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