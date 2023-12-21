using Arcatech.UI;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Arcatech.Units.Inputs
{
    public class AimingComponent : ManagedControllerBase
    {
        private Camera _camera;
        private Plane _plane;
        private Vector3 _mousePos;

        public SimpleEventsHandler<bool, BaseTargetableItem> SelectionUpdatedEvent;

        [Tooltip("Vertical offset for raycast plane"), SerializeField] private float _vertOffset = 0.1f;
        public Vector3 GetLookPoint => _mousePos; // used by inputs to rotate towards crosshair


        #region managed
        public override void StartController()
        {
            _plane = new Plane(Vector3.down, _vertOffset);
            _mousePos = transform.forward;
            _camera = Camera.main;
        }

        public override void UpdateController(float delta)
        {
            Ray r = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            // no object, aim at plane
            if (_plane.Raycast(r, out float rayDist))
            {
                _mousePos = r.GetPoint(rayDist);
                SelectionUpdatedEvent?.Invoke(false, null);
            }

            // hit a selectable
            if (Physics.Raycast(r, out var hit))
            {
                if (hit.collider.gameObject.TryGetComponent<BaseTargetableItem>(out var item))
                {
                    _mousePos = hit.collider.transform.position; // aim at the center of the target
                    SelectionUpdatedEvent?.Invoke(true, item);
                }
            }


        }

        public override void StopController()
        {

        }

        #endregion
    }

}