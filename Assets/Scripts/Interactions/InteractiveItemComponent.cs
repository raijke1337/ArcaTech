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
    [RequireComponent(typeof(EntityMouseOverGlowComponent))]
    public class InteractiveItemComponent : ValidatedMonoBehaviour, IInteractive, IStateAugmentor,ITriggerNotificationReceiver,IKillerComponent
    {
        [SerializeField] private Description itemDescription;
        [SerializeField] private bool itemDisappearsWhenUsed;
        [SerializeField] 
        private HandlersActivation handlersActivationType;
        
        [SerializeField, UnityEngine.Range(0, 59f)] private float useCooldown;
        private float _cd = 0;

        
        [Space,SerializeField, Self] private BaseGameEntityComponent baseComp;
        [SerializeField, Self] private EntityMouseOverGlowComponent entityMouseOver;
        [SerializeField] private TriggerTrackerComponent triggerTrackerComponent;
        [Space]
        
        [SerializeField] private List<InteractionHandlerBase> handlersOnThisItem;
        [SerializeField] private List<InteractionHandlerBase> handlers;

        private List<IInteractionHandler> _current;
        
        public BaseGameEntityComponent GetBaseComponent => baseComp;
        private List<IKillableComponent> killableComponents;


        private void Awake()
        {
            
            _current = new();
            handlersOnThisItem = new  List<InteractionHandlerBase>(GetComponentsInChildren<InteractionHandlerBase>());
            killableComponents =  new  List<IKillableComponent>(GetComponentsInChildren<IKillableComponent>());
            _current.AddRange(handlersOnThisItem);
            _current.AddRange(handlers);
        }

        private void Start()
        {
            triggerTrackerComponent.Active = true;
            triggerTrackerComponent.RegisterReceiver(this);
        }

        private void OnDisable()
        {
            triggerTrackerComponent.UnregisterReceiver(this);
        }


        #region interaction

        [Space, Header("Condition checker")]
        [SerializeField]
        protected InteractionCondition condition;

        private void OnEnable()
        {
            _transitions = new();

            var toS = interactionSuccess.Build();
            _successStateRef = toS.NextState;
            _transitions.Add(toS);
            
            var toF = interactionFail.Build();
            _failStateRef = toF.NextState;
            _transitions.Add(toF);
            
            _transitions.Add(interactStart.Build());
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
            if (!itemDisappearsWhenUsed) return;
            
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

        public void TriggerEntered(TriggerHitInfo triggerHitInfo)
        {
            if (!triggerHitInfo.IsValidHit) return;
            if (triggerHitInfo.Target.CompareTag("Player"))
            {
                if (triggerHitInfo.Target.ShowingDebugs) Debug.Log("Player enters interaction area");

                if (triggerHitInfo.Target.TryGetComponent(out EntityStateMachineComponent fsm))
                {
                    fsm.RegisterAugmentor(this);
                }

                if (triggerHitInfo.Target.TryGetComponent(out IInteractor interactor))
                {
                    interactor.RegisterInteractiveItem(this);
                }

                if (_current == null)
                {
                    Awake();
                }

                foreach (var h in _current)
                {
                    h.OnPlayerEnter();
                }
            }
        }

        public void TriggerExited(TriggerHitInfo triggerExitInfo)
        {
            if (!triggerExitInfo.IsValidHit) return;
            if (triggerExitInfo.Target.CompareTag("Player"))
            {
                if (triggerExitInfo.Target.TryGetComponent(out EntityStateMachineComponent fsm))
                {
                    fsm.UnregisterAugmentor(this);
                }
                if (triggerExitInfo.Target.TryGetComponent(out IInteractor interactor))
                {
                    interactor.UnregisterInteractiveItem(this);
                }
                foreach (var h in _current)
                {
                    h.OnPlayerExit();
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