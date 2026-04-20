using System;
using System.Collections.Generic;
using Arcatech.Managers;
using Arcatech.Texts;
using Arcatech.Triggers;
using Arcatech.Units;
using UnityEngine;
using KBCore.Refs;
using UnityEngine.EventSystems;

namespace Arcatech.Interactions
{
    [RequireComponent(typeof(BaseGameEntityComponent))]
    public abstract class EventsTrigger : ValidatedMonoBehaviour,ITriggerNotificationReceiver
    {
        [Space,SerializeField, Self] protected BaseGameEntityComponent baseComp;
        [SerializeField,Child] protected TriggerTrackerComponent activationArea;
        
        [SerializeField] protected bool disappearWhenTriggered;
        
        [SerializeField] protected bool allowMultipleActivations = false;
        public abstract void TriggerEntered(TriggerHitInfo triggerHitInfo);
        public abstract void TriggerExited(TriggerHitInfo triggerExitInfo);
        
        
        protected virtual void Start()
        {
            activationArea.Active = true;
            activationArea.RegisterReceiver(this);
        }

        protected virtual void OnDisable()
        {
            activationArea.UnregisterReceiver(this);
        }
    }
    

    [RequireComponent(typeof(EntityMouseOverGlowComponent))]
    public class InteractiveEventsTriggerComponent : EventsTrigger, IInteractive, IStateAugmentor,IKillerComponent
    {
        [SerializeField] private Description itemDescription;
        [SerializeField] 
        private HandlersActivation handlersActivationType;
        
        [SerializeField, UnityEngine.Range(0, 59f)] private float useCooldown;
        private float _cd = 0;

        

        [SerializeField, Self] private EntityMouseOverGlowComponent entityMouseOver;
        [Space]
        
        [SerializeField] private List<InteractionEventHandlerBase> interactionEventsOnThis;
        [SerializeField] private List<InteractionEventHandlerBase> otherInteractionEvents;

        private List<IActiveInteractionHandler> _current;
        
        public BaseGameEntityComponent GetBaseComponent => baseComp;
        private List<IKillableComponent> killableComponents;

        protected override void OnValidate()
        {
            base.OnValidate();
            interactionEventsOnThis = new  List<InteractionEventHandlerBase>(GetComponentsInChildren<InteractionEventHandlerBase>());
        }

        private void Awake()
        {
            
            _current = new();
            interactionEventsOnThis = new  List<InteractionEventHandlerBase>(GetComponentsInChildren<InteractionEventHandlerBase>());
            killableComponents =  new  List<IKillableComponent>(GetComponentsInChildren<IKillableComponent>());
            _current.AddRange(interactionEventsOnThis);
            _current.AddRange(otherInteractionEvents);
        }




        #region interaction

        [Space, Header("Condition checker")]
        [SerializeField]
        protected InteractionCondition condition;

        private void OnEnable()
        {
            _transitions = new();

            var toS = interactionSuccess?.Build();
            if (toS != null)
            {
                _successStateRef = toS.NextState;
                _transitions.Add(toS);
            }

            var toF = interactionFail?.Build();
            if (toF != null)
            {
                _failStateRef = toF.NextState;
                _transitions.Add(toF);
            }

            _transitions.Add(interactStart?.Build());
        }

        private void Update()
        {
            _cd = Mathf.Clamp(_cd-Time.deltaTime, 0, useCooldown);
        }

        public bool TryInteraction(IInteractor interactor) => condition.CheckCondition(interactor, this);

        #endregion

        public void OnPointerEnter(PointerEventData eventData)
        {
            GameInterfaceManager.Instance?.NotifyTargetable(this,true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            GameInterfaceManager.Instance?.NotifyTargetable(this,false);
        }

        
        #region state

        [SerializeField] private SerializedStateTransition interactStart;
        [SerializeField] private SerializedStateTransition interactionSuccess;
        [SerializeField] private SerializedStateTransition interactionFail;

        private UnitState _successStateRef;
        private UnitState _failStateRef;
        
        
        private List<StateTransition> _transitions;
        
        public void Attach(IStateAugmentorReceiver machine)
        {
            foreach (var s in _transitions)
            {
                machine.AddTransition(s);
            }
        }

        public void Detach(IStateAugmentorReceiver machine)
        {
            foreach (var s in _transitions)
            {
                machine.RemoveTransition(s);
            }
        }

        public void OnStateEntered(UnitState state, StateMachineContext context)
        {
            if (state == _successStateRef || state == _failStateRef)
            {
                context.Interactor.InteractionContext.ConsumeInteractionResult(out _);
            }

            if (handlersActivationType == HandlersActivation.OnEnterState)
            {
                if (state == _failStateRef)
                {
                    foreach (var handler in _current)
                    {
                        handler.DoInteraction(false,context.Interactor);
                    }
                }
                if (state == _successStateRef)
                {
                    foreach (var handler in _current)
                    {
                        handler.DoInteraction(true, context.Interactor);
                    }
                }
            }
        }

        public void OnStateExited(UnitState state, StateMachineContext context)
        {
            if (handlersActivationType == HandlersActivation.OnExitState)
            {
                if (state == _failStateRef)
                {
                    foreach (var handler in _current)
                    {
                        handler.DoInteraction(false,context.Interactor);
                    }
                }
                if (state == _successStateRef)
                {
                    foreach (var handler in _current)
                    {
                        handler.DoInteraction(true,context.Interactor);
                    }
                }
            }

            if (state != _successStateRef) return;
            if (!disappearWhenTriggered) return;
            
            var fsm = context.Owner.GetComponent<EntityStateMachineComponent>();
            fsm.UnregisterAugmentor(this);
            foreach (var k in killableComponents)
            {
                k.SetKilled(this,true);
            }
        }

        #endregion

        #region Description


        private Description _setDescription;
        public Description GetInfo => _setDescription == null ? itemDescription : _setDescription;
        public void SetDescription(Description description) => _setDescription = description;
        #endregion

        public override void TriggerEntered(TriggerHitInfo triggerHitInfo)
        {
            if (triggerHitInfo.TargetCollider.CompareTag("Player"))
            {
                if (triggerHitInfo.TargetCollider.TryGetComponent(out EntityStateMachineComponent fsm))
                {
                    fsm.RegisterAugmentor(this);
                }

                if (triggerHitInfo.TargetCollider.TryGetComponent(out IInteractor interactor))
                {
                    interactor.RegisterInteractiveItemInContext(this);
                }

                if (_current == null)
                {
                    Awake();
                }
            }
        }

        public override  void TriggerExited(TriggerHitInfo triggerExitInfo)
        {
            if (triggerExitInfo.TargetCollider.CompareTag("Player"))
            {
                if (triggerExitInfo.TargetCollider.TryGetComponent(out EntityStateMachineComponent fsm))
                {
                    fsm.UnregisterAugmentor(this);
                }
                if (triggerExitInfo.TargetCollider.TryGetComponent(out IInteractor interactor))
                {
                    interactor.UnregisterInteractiveItemFromContext(this);
                }
            }
        }

        public string KilledBy => $"Interactive item {name}";
    }
    
    public enum HandlersActivation
    {
        OnEnterState,
        OnExitState
    }
}