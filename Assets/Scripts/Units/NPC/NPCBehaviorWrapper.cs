using System;
using Arcatech.Stats;
using Arcatech.Triggers;
using Arcatech.Units.Control;
using KBCore.Refs;
using NUnit.Framework;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

namespace Arcatech.Units
{
    /// <summary>
    /// this is now separate from ActiveUnitComp (aka StateMachine)
    /// </summary>
    [RequireComponent(typeof(BehaviorGraphAgent),typeof(NavMeshAgent),typeof(UnitInputsComponent))]
    [RequireComponent(typeof(BaseGameEntityComponent),typeof(EntityStatsComponent))]
    public class NPCBehaviorWrapper : ValidatedMonoBehaviour, IAppliedEffectsTakerComponent<AppliedStatsDeltaEffect>,IKillableComponent,IPausableComponent,IMove,IStatUpdatesViewer
    {
        [SerializeField,Self]protected NavMeshAgent agent;
        [SerializeField,Self]protected BehaviorGraphAgent behavior;
        [SerializeField,Self]protected BaseGameEntityComponent entity;
        [SerializeField,Self]protected UnitInputsComponent unitInputs;
        
        
        [Space, Header("Stats"),SerializeField,Self]
        protected EntityStatsComponent stats;

        [SerializeField] private readonly string hpParamaterName = "CurrentHP";
        [SerializeField] private readonly string energyParamaterName = "CurrentEnergy";
        [SerializeField] private readonly string staminaParamaterName = "CurrentStamina";
        
        
        private BlackboardReference bbref;
        EnterCombatEventChannel combatEventChannel;

        public bool CombatState
        {
            get
            {
                bbref.GetVariableValue("CombatState", out bool result);
                return result;  
            }
            set => combatEventChannel.SendEventMessage(value);
        }

         
        
        protected void Start()
        {
            if (behavior)
            {
                // failsafe for unset units
                bbref = behavior.BlackboardReference;
                bbref.GetVariableValue("PlayerAttackedEvent", out combatEventChannel);
                Assert.IsNotNull(combatEventChannel);

                bbref.SetVariableValue(hpParamaterName, 1f);
                bbref.SetVariableValue(energyParamaterName, 1f);
                bbref.SetVariableValue(staminaParamaterName, 1f);
            }
            if (stats)
            {
                stats.RegisterStatsViewer(this);
            }
            // 
        }
        public void ApplyEffect(AppliedStatsDeltaEffect effect,BaseGameEntityComponent source)
        {
            if (source == null) return;
            if (source.GetEntitySide != entity.GetEntitySide && entity.GetEntitySide != Side.Unassigned)
            {
                CombatState = true;
               // Debug.Log($"{GetMainEntity.GetName} received {effect} from {source}, entering combat");
            }
        }

        public void SetKilled(IKillerComponent component, bool value)
        {
            agent.isStopped = value;
            if (!behavior) return;
            if (value) behavior.End(); else behavior.Restart();
        }
        
        private bool _paused;
        public bool Paused
        {
            get => _paused;
            set
            {
                _paused = value;
                // base.OnPause(paused);   
                agent.isStopped = _paused;
            
                if (!behavior) return;
                behavior.enabled = !_paused;
                
            }
        }

        private bool _canMove;
        public bool CanMove
        {
            get => _canMove;
            set
            {
                _canMove = value;
                agent.isStopped = !value;
            }
        }
        public Vector3 MovementVector { get; set; }
        public float ActualMovementVelocity => agent.velocity.magnitude;
        public bool IsGrounded => agent.isOnNavMesh;
        public bool UseRootMotion { get; set; }
        public void HandleStatsUpdate(ResourceStatType stat, float statCurrent, float statMax, float statDelta, object changeSource)
        {
            switch (stat)
            {
                case ResourceStatType.Health:
                    bbref.SetVariableValue(hpParamaterName, statCurrent / statMax);
                    break;
                case ResourceStatType.Stamina:
                    bbref.SetVariableValue(staminaParamaterName, statCurrent / statMax);
                    break;
                case ResourceStatType.Energy:
                    bbref.SetVariableValue(energyParamaterName, statCurrent / statMax);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stat), stat, null);
            }
        }
    }
}

