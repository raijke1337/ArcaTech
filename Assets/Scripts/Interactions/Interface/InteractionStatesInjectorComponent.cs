using System;
using System.Collections;
using System.Collections.Generic;
using Arcatech.Units;
using AYellowpaper.SerializedCollections;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Interactions
{
    public class InteractionStatesInjectorComponent : ValidatedMonoBehaviour, IStateAugmentor
    {
        
        [SerializeField] SerializedDictionary<InteractionState,SerializedStateTransition> stateTransitions;
        private Dictionary<InteractionState, StateTransition> _transitions;

        private void Start()
        {
            _transitions = new Dictionary<InteractionState, StateTransition>();
            foreach (var pair in stateTransitions)
            {
                _transitions[pair.Key] = pair.Value.Build();
            }
        }

        #region State Augmentor
        public void Attach(IStateAugmentorReceiver machine)
        {
            machine.RegisterAugmentor(this);
            foreach (var pair in _transitions)
            {
                machine.AddTransition(pair.Value);
            }
        }

        public void Detach(IStateAugmentorReceiver machine)
        {
            machine.UnregisterAugmentor(this);
            foreach (var pair in _transitions)
            {
                machine.RemoveTransition(pair.Value);
            }
        }

        public void OnStateEntered(UnitState state, StateMachineContext context)
        { }

        public void OnStateExited(UnitState state, StateMachineContext context)
        { }
        
        #endregion
    }
}