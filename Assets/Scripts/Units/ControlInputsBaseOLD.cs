using Arcatech.Triggers;
using System;
using UnityEngine;

namespace Arcatech.Units
{

    public abstract class ControlInputsBaseOLD : MonoBehaviour, IManagedController
    {
        public Vector3 InputsMovementVector { get; protected set; }
        public Vector3 InputsLookVector { get; protected set; }


        protected abstract ControlInputsBaseOLD ControllerBindings(bool start);
        public event Action<UnitActionType> UnitActionRequestedEvent = delegate { };
        public event Action InputsPause = delegate { };
        public event Action<IInteractible> RequestInteraction = delegate { };

        protected virtual void RequestCombatAction(UnitActionType type)
        {
            UnitActionRequestedEvent.Invoke(type);
        }
        
        protected void CallBackPause() => InputsPause.Invoke();
        protected void CallBackInteraction(IInteractible i) => RequestInteraction.Invoke(i);

        public virtual void StartController()
        {
            ControllerBindings(true);
        }

        public virtual void ControllerUpdate(float delta)
        {

        }

        public virtual void FixedControllerUpdate(float fixedDelta)
        {
            
        }

        public virtual void StopController()
        {
            ControllerBindings(false);
        }
    }
}