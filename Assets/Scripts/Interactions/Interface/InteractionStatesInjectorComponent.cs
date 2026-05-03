using System;
using System.Collections;
using System.Collections.Generic;
using Arcatech.Triggers;
using Arcatech.Units;
using AYellowpaper.SerializedCollections;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Interactions
{
    public class InteractionStatesInjectorComponent : ValidatedMonoBehaviour, IStateAugmentor, ITriggerNotificationReceiver
    {
        [SerializeField] private TriggerTrackerComponent activeArea;
        [SerializeField] SerializedDictionary<InteractionState,SerializedStateTransition> stateTransitions;
        private Dictionary<InteractionState, StateTransition> _transitions;

        private void Start()
        {
            _transitions = new Dictionary<InteractionState, StateTransition>();
            foreach (var pair in stateTransitions)
            {
                _transitions[pair.Key] = pair.Value.Build();
            }
            activeArea?.RegisterReceiver(this);
        }

        private void OnDisable()
        {
            activeArea?.UnregisterReceiver(this);
        }

        #region State Augmentor
        public void Attach(IStateAugmentorReceiver machine)
        {
            foreach (var pair in _transitions)
            {
                machine.AddTransition(pair.Value);
            }
        }

        public void Detach(IStateAugmentorReceiver machine)
        {
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

        public void TriggerEntered(TriggerHitInfo triggerHitInfo)
        {
            if (triggerHitInfo.TryGetEntityTarget(out var target) &&
                target.TryGetComponent(out IStateAugmentorReceiver receiver))
            {
                receiver.RegisterAugmentor(this);
            }
        }

        public void TriggerExited(TriggerHitInfo triggerExitInfo)
        {
            if (triggerExitInfo.TryGetEntityTarget(out var target) &&
                target.TryGetComponent(out IStateAugmentorReceiver receiver))
            {
                receiver.UnregisterAugmentor(this);
            }
        }
    }
}