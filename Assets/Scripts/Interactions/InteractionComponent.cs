using System;
using System.Linq;
using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Serialization;

namespace Arcatech.Interactions
{
    [RequireComponent(typeof(ActiveGameUnitComponent))]
    public class InteractionComponent : ValidatedMonoBehaviour, IInteractor
    {
        [SerializeField, Range(0, 5)] private float interactRange = 3f;
        [SerializeField, Range(0.016f,1),Tooltip("seconds per 1 scan, min is each frame")] private float scanFrequency = 0.2f;
        [SerializeField] private SerializedUnitState stateSuccess;
        [SerializeField] private SerializedUnitState stateFail;
        
        
        private IInteractive currentInteractive;
        [Space,SerializeField,Self] ActiveGameUnitComponent cachedActor;
        [SerializeField,Tooltip("effects spawn here")] private Transform interactionActionTransform;

        private UnitState _successState;
        private UnitState _failState;

        private float time = 0f;



        private void FixedUpdate()
        {
            time += Time.fixedDeltaTime;
            if (time >= scanFrequency)
            {
                time = 0f;
                var colliders = Physics.OverlapSphere(transform.position, interactRange);
                foreach (var collider in colliders)
                {
                    if (collider.TryGetComponent(out IInteractive interactive))
                    {
                        currentInteractive = interactive;
                        break;
                    }

                }

                if (!colliders.Any(t => t.TryGetComponent(out IInteractive interactive)))
                {
                    currentInteractive = null;
                } 

            }
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
            if (cachedActor == null)
            {
                if (stateSuccess != null)
                {
                    _successState = stateSuccess.DeserializeState(cachedActor, interactionActionTransform);
                }

                if (stateFail != null)
                {
                    _failState = stateFail.DeserializeState(cachedActor, interactionActionTransform);
                }
            }

            if (currentInteractive != null)
            {
                bool ok = currentInteractive.OnInteraction(this);
                if (_successState != null && _failState != null)
                    cachedActor.ForceUnitState(ok ? _successState : _failState);
            }
            else
            {
                Debug.Log($"no interactive item nearby");
            }
        }
    }
}
