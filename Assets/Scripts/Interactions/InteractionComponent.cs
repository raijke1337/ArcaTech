using System;
using System.Linq;
using Arcatech.Items;
using Arcatech.Units;
using Arcatech.Units.Control;
using KBCore.Refs;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Arcatech.Interactions
{
    [RequireComponent(typeof(EntityStateMachineComponent), typeof(PlayerAimingComponent))]
    public class InteractionComponent : ValidatedMonoBehaviour, IInteractor, IUnitCommandValidator,
        IUnitCommandPerformer
    {
        [SerializeField, Range(0, 5)] private float interactRange = 1.5f;


        [SerializeField] private SerializedStateTransition stateSuccess;
        [SerializeField] private SerializedStateTransition stateFail;


        [Space, SerializeField, Self] EntityStateMachineComponent stateMachine;

        [SerializeField, Tooltip("effects spawn here")]
        private Transform interactionActionTransform;

        private IInteractionTargetPicker _aim;
        private float time = 0f;

        private void Start()
        {

            stateMachine.AddTransition(stateSuccess.Build());
            stateMachine.AddTransition(stateFail.Build());

            _aim = GetComponentInChildren<IInteractionTargetPicker>();
            if (_aim == null) Debug.LogError("No IInteractionTargetPicker component found on " + gameObject.name);
        }


        private InteractionContext context;

        private InteractionContext ReadContext()
        {
            if (context == null) context = new InteractionContext(stateMachine.GetMainEntity, interactionActionTransform);
            return context;
        }

        public InteractionContext InteractionContext => ReadContext();

        public bool Paused { get; set; }

        private bool HasInteractiveItemInRange(IInteractive item)
        {
            if (item == null) return false;
            return Vector3.Distance(transform.position,item.GetBaseComponent.transform.position) <= interactRange;
        }

        private bool HasInteractiveItemSelected(out IInteractive item)
        {
            return _aim.HasInteractiveSelected(out item);
        }
        
        public bool CanDoUnitCommand(UnitActionType type, out string info)
        {
            info = "Paused";
            if (Paused) return false;
            
            switch (type)
            {
                case UnitActionType.Use:
                    info = HasInteractiveItemSelected(out var item) ? "OK / " : "No interactive selected / ";
                    info += HasInteractiveItemInRange(item) ? "OK" : "Interactive not in range";
                    return HasInteractiveItemSelected(out item) && HasInteractiveItemInRange(item);
                    
                    default: return true;
            }
        }

        public bool DoUnitCommand(UnitActionType type, bool wasSuccessful)
        {
            if (!wasSuccessful) return false;
            if (type == UnitActionType.Use && CanDoUnitCommand(type, out _))
            {
                InteractionContext.UpdateInteractionResult(_aim.DoInteraction(this));
            }
            return true;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, interactRange);
        }
    }
}
