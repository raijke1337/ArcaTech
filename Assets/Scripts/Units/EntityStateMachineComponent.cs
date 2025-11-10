using System.Collections.Generic;
using KBCore.Refs;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Arcatech.Units
{
    /// <summary>
    /// new component to define a unit that has some state (e.g. idle, attacking, stunned...)
    /// basically TODO: upgrading this into a proper state machine with control over animator
    /// </summary>
    [RequireComponent(typeof(BaseGameEntityComponent))]

    public class EntityStateMachineComponent : ValidatedMonoBehaviour, IPausableComponent,IKillableComponent
    {
        private static readonly int ExitStateTrigger = Animator.StringToHash("ExitStateTrigger");
        [SerializeField, Self] BaseGameEntityComponent gameEntity;
        [SerializeField, Child] protected Animator animator;
    

        [Space, Header("States")] [SerializeField]
        private string animatorExitStateTrigger = "ExitStateTrigger";
        [SerializeField] SerializedUnitState StaggeredState;
        [SerializeField] SerializedUnitState DeadState;
        [SerializeField] SerializedUnitState StunnedState;

        private StateMachineContext _context;
        
        private int exitStateHash;
        protected UnitState _staggerState;
        protected UnitState _deathState;
        protected UnitState _stunnedState;
        
        SimpleEntityShadowComponent _entityShadowComponent;

        public BaseGameEntityComponent GetMainEntity { get => gameEntity; }


        protected virtual void Start()
        {
            _context = new StateMachineContext() { Spawn = gameEntity.SpawnPoint };
            
            exitStateHash = Animator.StringToHash(animatorExitStateTrigger);
            // these TODO: move into stats component
            // if (StaggeredState) _staggerState = StaggeredState.DeserializeState(this,transform);
            // if (DeadState) _deathState = DeadState.DeserializeState(this, transform);
            // if (StunnedState) _stunnedState = StunnedState.DeserializeState(this,transform);

        }

        private void Update()
        {
            if (Paused) return;
            CurrentState?.UpdateState(Time.deltaTime);
        }
        

        #region actions

        private UnitState CurrentState;

        public void ForceUnitState(UnitState nextState, bool instant = false)
        {
            if (Paused || nextState == null) return;
            
            CurrentState = nextState;
            CurrentState.StartState(_context,animator);
        }

        #endregion


        #region ipausable

        private bool _p;

        public bool Paused { get; set; }

        #endregion
        

        #region IKillable

        private bool _k;

        public bool Killed
        {
            get => _k;
            set => OnKill(value);
        }

        protected virtual void OnKill(bool kill)
        {
            Paused = kill;
            _k = kill;
            _deathState?.StartState(new StateMachineContext(),animator); 
            if (!_k) Debug.Log($"Trying to resurrect {this} and its NYI. You can't bring back the dead...");
        }

        #endregion

        
    }

    /// <summary>
    /// this interface will be looked up by the state machine to collect all states for unit
    /// </summary>
    public interface IUnitStateProvider
    {
        public IEnumerable <SerializedUnitState> GetStates { get; }
    }
}