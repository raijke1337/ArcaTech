using System;
using System.Linq;
using Arcatech.Units;
using UnityEngine;
using UnityEngine.Serialization;

namespace Arcatech.Interactions
{
    public class InteractionComponent : MonoBehaviour, IInteractor
    {
        [SerializeField, Range(0, 5)] private float interactRange = 3f;
        [SerializeField, Range(0.016f,1),Tooltip("seconds per 1 scan, min is each frame")] private float scanFrequency = 0.2f;
        [SerializeField] private SerializedUnitState stateSuccess;
        [SerializeField] private SerializedUnitState stateFail;
        
        
        private IInteractive currentInteractive;
        private ActiveGameUnitComponent cachedActor;
        private Transform cachedTransform;

        private UnitState _successState;
        private UnitState _failState;

        private float time = 0f;

        private void Start()
        {
            cachedTransform =  transform;
        }

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

        public void DoInteraction(InteractionContext context)
        {
            if (cachedActor == null)
            {
                cachedActor = context.ActiveGameUnitComponent;
                if (stateSuccess != null)
                {
                    _successState = stateSuccess.ProduceAction(cachedActor, cachedTransform);
                }

                if (stateFail != null)
                {
                    _failState = stateFail.ProduceAction(cachedActor, cachedTransform);
                }
            }

            if (currentInteractive != null)
            {
                bool ok = currentInteractive.OnInteraction(this, context);
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
