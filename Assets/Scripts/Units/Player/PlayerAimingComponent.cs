using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Units.Control
{

    public class PlayerAimingComponent : MonoBehaviour, IPausableComponent, IKillableComponent
    {
        public void SetKilled(IKillerComponent comp, bool value) => _killed = value;

        public bool Paused { get; set; } = false;
        
        private void Start()
        {
            // Initialize the plane at y=0 initially
            groundPlane = new Plane(Vector3.up, 0f);
            aimInterfaces.AddRange(GetComponents<IAim>());
        }

        private void Update()
        {
            if (Paused || _killed) return;
            DoAiming();
        }
        
        

        private Vector3 desiredLookDirection;
        private Vector3 _aimPosition;
        [SerializeField] float aimOffset = 0f;

        private List<IAim> aimInterfaces = new List<IAim>();

        private Plane groundPlane;
        private bool _killed = false;


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

            }
            

            // Flatten to XZ plane for top-down isometric calculation
            Vector3 currentFlat = _aimPosition;
            currentFlat.y = 0f;
            currentFlat.Normalize();

            Vector3 desiredFlat = desiredLookDirection;
            desiredFlat.y = 0f;
            desiredFlat.Normalize();

            foreach (var aim in aimInterfaces)
            {
                aim.AimPosition = desiredLookDirection;
            }
        }

 
    }
}