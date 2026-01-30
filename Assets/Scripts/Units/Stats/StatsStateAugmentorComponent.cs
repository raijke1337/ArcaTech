using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Stats
{
    /// <summary>
    /// moved the augment logic to this for clarity
    /// </summary>
    [RequireComponent(typeof(EntityStatsComponent),typeof(EntityStateMachineComponent))]
    public class StatsStateAugmentorComponent : ValidatedMonoBehaviour, IStateAugmentor, IKillerComponent, IAppliedEffectsTakerComponent<AppliedStatsDeltaEffect>
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
            //
            // if (_damageInterrupt != null)
            // {
            //     _damageInterrupt ??= toDamageInterrupt.Build();
            //     _damage = _damageInterrupt.NextState;
            //     machine.AddTransition(_damageInterrupt);
            // }
            
            _stateMachineCtx = machine.Context; // TODO: set some trigger for stagger /instead of plain stat condition
        }

        public void Detach(IStateAugmentorReceiver machine)
        {
            if (_toKilled != null) machine.RemoveTransition(_toKilled);
            if (_toKnockDown != null) machine.RemoveTransition(_toKnockDown);
            if (_damageInterrupt!=null) machine.RemoveTransition(_damageInterrupt);
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
            if (state == _damage)
            {
                context.Animator.SetFloat(dmgFrontHash,0);
                context.Animator.SetFloat(dmgRightHash,0);
            }
        }

        public string KilledBy => "Entered DeadState in StateMachine";
        #region dmg take state

        public bool ApplyEffect(AppliedStatsDeltaEffect effect, BaseGameEntityComponent source)
        {
            if (!canBeInterrupted) return false;
            
            if (stats.CheckStatsConditionGroup(interruptCondition))
            {
                
            }
            return true;
        }

        [SerializeField] private bool canBeInterrupted = true;
        [SerializeField] private ConditionGroup interruptCondition;
        [SerializeField] private SerializedStateTransition toDamageInterrupt;
        [SerializeField] private string dmgFrontAnimatorParameter = "DamageFront";
        [SerializeField] private string dmgRightAnimatorParameter = "DamageRight";

        private int dmgFrontHash;
        private int dmgRightHash;

        private void Awake()
        {
            dmgFrontHash = Animator.StringToHash(dmgFrontAnimatorParameter);
            dmgRightHash = Animator.StringToHash(dmgRightAnimatorParameter);
        }



        private StateTransition _damageInterrupt;
        private UnitState _damage;

        
        #endregion

    }
}