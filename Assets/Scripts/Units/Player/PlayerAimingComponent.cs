using System.Collections.Generic;
using Arcatech.Interactions;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Units.Control
{

    public class PlayerAimingComponent : ValidatedMonoBehaviour, IPausableComponent, IKillableComponent,
        IInteractionTargetPicker
    {
        public bool Killed { get; set; } = false;
        public bool Paused { get; set; } = false;

        
        
        
        private void Start()
        {
            animatorSignedAngleHash =  Animator.StringToHash(aimRotationAngleAnimatorString);
            // Initialize the plane at y=0 initially
            groundPlane = new Plane(Vector3.up, 0f);
            aimInterfaces.AddRange(GetComponents<IAim>());
        }

        private void Update()
        {
            if (Paused || Killed) return;
            DoAiming();
            InteractionRaycast();
        }
        


        #region aiming

        private Vector3 desiredLookDirection;
        private Vector3 _aimPosition;
        [SerializeField] float aimOffset = 0f;

        private List<IAim> aimInterfaces = new List<IAim>();

        private Plane groundPlane;
        
        


        private string aimRotationAngleAnimatorString = "SignedAngle";
        private int animatorSignedAngleHash;

        private void DoAiming()
        {
            groundPlane.distance = -(transform.position.y + aimOffset);


            Vector3 mousePosition = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);

            // Raycast to ground plane to find intersection point
            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);

                // Compute desired direction from character's position to hit point
                desiredLookDirection = (hitPoint - transform.position).normalized;

                // Store for gizmo
                _aimPosition = hitPoint;
            }
            else
            {
                // Fallback: if no hit, keep previous direction or set to zero
                desiredLookDirection = Vector3.zero;

                // No update to _aimPosition
            }
            

            // Flatten to XZ plane for top-down isometric calculation
            Vector3 currentFlat = _aimPosition;
            currentFlat.y = 0f;
            currentFlat.Normalize();

            Vector3 desiredFlat = desiredLookDirection;
            desiredFlat.y = 0f;
            desiredFlat.Normalize();

            // Calculate signed angle around the up axis (positive = clockwise when viewed from above)
            float signedAngle = Vector3.SignedAngle(currentFlat, desiredFlat, Vector3.up);
           // animator.SetFloat(animatorSignedAngleHash, signedAngle);


            foreach (var aim in aimInterfaces)
            {
                aim.AimPosition = desiredLookDirection;
            }
        }

        private void OnDrawGizmos()
        {
            // Draw a sphere at the current aim position for visualization
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_aimPosition, 0.1f);
        }
    


    #endregion
        #region Interaction Picker

        [Header("Interaction")]
        [SerializeField] LayerMask targetingLayerMask;
        [SerializeField] private readonly float raycastTimer = 0.3f;

        private CountDownTimer _interactionRaycastTimer;
        private float timer;
        private IInteractive selected;
        
        private void InteractionRaycast()
        {
            if (timer < raycastTimer)
            {
                timer +=  Time.deltaTime;
                return;
            }
            timer = 0;
            
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, targetingLayerMask.value))
            {
                if (hit.collider.TryGetComponent(out IInteractive inter))
                {
                    selected = inter;
                }
                else
                {                
                    selected = null;
                }
            }
        }
        


        public bool HasInteractiveSelected(out IInteractive item)
        {
            item = selected;
            return selected != null;
        }

        public bool DoInteraction(IInteractor interactor)
        {
            if (selected == null) return false;
            return selected.TryInteraction(interactor);
        }
        
        #endregion
    }
}