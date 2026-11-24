using System;
using System.Collections;
using System.Collections.Generic;
using Arcatech.Triggers;
using Arcatech.Units;
using AYellowpaper.SerializedCollections;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Interactions
{
    public class InteractionStatesInjectorComponent : MonoBehaviour, IStateAugmentor, ITriggerNotificationReceiver
    {
        [SerializeField] private BaseGameEntityComponent ownerEntity; // ← носитель (враг)
        [SerializeField] private TriggerTrackerComponent activeArea;
        [SerializeField] SerializedDictionary<InteractionState, SerializedStateTransition> stateTransitions;

        private Dictionary<InteractionState, StateTransition> _transitions;
        private Dictionary<string, InteractionState> _stateNames;
        private readonly HashSet<IStateAugmentorReceiver> _attachedTo = new();

        public UnityEvent<InteractionState> StateAnimationExited;

        private void Start()
        {
            ownerEntity = GetComponent<BaseGameEntityComponent>();
            if (ownerEntity == null) ownerEntity = gameObject.GetComponentInParent<BaseGameEntityComponent>();
            
            _transitions = new Dictionary<InteractionState, StateTransition>();
            _stateNames = new();
            foreach (var pair in stateTransitions)
            {
                _transitions[pair.Key] = pair.Value.Build();
                _stateNames[pair.Value.nextState.stateDisplayName] = pair.Key;
            }

            activeArea?.RegisterReceiver(this);

            if (ownerEntity != null)
                ownerEntity.AnnounceDead.AddListener(OnOwnerDied);
        }

        private void OnDisable()
        {
            activeArea?.UnregisterReceiver(this);
            DetachFromAll();
        }

        private void OnDestroy()
        {
            if (ownerEntity != null)
                ownerEntity.AnnounceDead.RemoveListener(OnOwnerDied);
        }

        private void OnOwnerDied(BaseGameEntityComponent _)
        {
            // Враг умер — снимаем свои transition-ы у всех, кому раздали.
            // Иначе у игрока в стейт-машине останется висеть переход в анимацию добивания.
            DetachFromAll();
        }

        private void DetachFromAll()
        {
            if (_transitions == null) return;
            foreach (var receiver in _attachedTo)
            {
                if (receiver == null) continue;
                foreach (var pair in _transitions)
                    receiver.RemoveTransition(pair.Value);
            }

            _attachedTo.Clear();
        }

        #region State Augmentor

        public void Attach(IStateAugmentorReceiver machine)
        {
            if (_transitions == null) return;
            foreach (var pair in _transitions)
                machine.AddTransition(pair.Value);
            _attachedTo.Add(machine);
        }

        public void Detach(IStateAugmentorReceiver machine)
        {
            if (_transitions == null) return;
            foreach (var pair in _transitions)
                machine.RemoveTransition(pair.Value);
            _attachedTo.Remove(machine);
        }

        public void OnStateEntered(UnitState state, StateMachineContext context)
        {
        }

        public void OnStateExited(UnitState state, StateMachineContext context)
        {
            if (_stateNames.TryGetValue(state.StateName, out var s))
                StateAnimationExited?.Invoke(s);
        }

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

        public bool TryGetTransitionByInteractionState(InteractionState state, out SerializedStateTransition transition)
        {
            return stateTransitions.TryGetValue(state, out transition);
        }
    }
}