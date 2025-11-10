using System;
using System.Linq;
using Arcatech.Units;
using Arcatech.Units.Control;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Serialization;

namespace Arcatech.Interactions
{
    [RequireComponent(typeof(EntityStateMachineComponent), typeof(PlayerAimingComponent))]
    public class InteractionComponent : ValidatedMonoBehaviour, IInteractor
    {
        [SerializeField, Range(0, 5)] private float interactRange = 1.5f;
       // [SerializeField, Range(0.016f,1),Tooltip("seconds per 1 scan, min is each frame")] private float scanFrequency = 0.2f;
       
       [SerializeField] private SerializedUnitState stateSuccess;
        [SerializeField] private SerializedUnitState stateFail;
        
        
        [Space,SerializeField,Self] EntityStateMachineComponent cachedActor;
        [SerializeField,Tooltip("effects spawn here")] private Transform interactionActionTransform;
        
        
        private IInteractionTargetPicker _aim;
        
        private UnitState _successState;
        private UnitState _failState;

        private float time = 0f;

        private void Start()
        {
            if (!cachedActor) cachedActor = GetComponentInChildren<EntityStateMachineComponent>();
            _aim = GetComponentInChildren<IInteractionTargetPicker>();
            if (_aim == null) Debug.LogError("No IInteractionTargetPicker component found on " + gameObject.name);
        }


        private InteractionContext context;
        private InteractionContext ReadContext()
        {
            context ??= InteractionContext.Create(cachedActor, interactionActionTransform, "Default context");
            return context;
        }
        
        public InteractionContext InteractionContext => ReadContext();
        public void InteractCommand()
        {
            if (_aim.DesiredInteractiveItem == null || Paused) return;
            if (stateSuccess != null)
            {
                _successState = stateSuccess.DeserializeState(cachedActor, interactionActionTransform);
            }

            if (stateFail != null)
            {
                _failState = stateFail.DeserializeState(cachedActor, interactionActionTransform);
            }
            if (_aim.DesiredInteractiveItem != null && Vector3.Distance(transform.position,_aim.DesiredInteractiveItem.GetBaseComponent.transform.position)<= interactRange)
            {
                bool ok = _aim.DesiredInteractiveItem.TryInteraction(this);
                if (_successState != null && _failState != null)
                    cachedActor.ForceUnitState(ok ? _successState : _failState);
            }
        }

        private void OnDrawGizmos()
        {
            if (_aim?.DesiredInteractiveItem == null)
            {
                Gizmos.color = Color.gray;
            }
            else
            {
                Gizmos.color = Color.blue;
            }

            Gizmos.DrawWireSphere(transform.position, interactRange);
        }

        public bool Paused { get; set; }
    }
}
