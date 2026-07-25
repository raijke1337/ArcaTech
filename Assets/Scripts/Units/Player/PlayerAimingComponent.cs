using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Units.Control
{
    public interface IAim
    {
        bool CanAim { get; set; }
        Vector3 AimDirection { get; set; }
    }

    public interface IAimTarget
    {
        Vector3 GetAimPoint();
    }

    public class PlayerAimingComponent : MonoBehaviour, IPausableComponent, IKillableComponent
    {
        public void SetKilled(IKillerComponent comp, bool value) => _killed = value;
        public bool Paused { get; set; } = false;

        [SerializeField] private float aimOffset = 0f;
        [SerializeField] private LayerMask entitiesLayerMask;

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;   // включить в инспекторе для отладки

        private Vector3 _desiredLookDirection;
        private List<IAim> _aimInterfaces = new List<IAim>();
        private Plane _groundPlane;
        private bool _killed = false;

        private CamerasController _cameraProvider;
        private Camera _camera;

        public BaseGameEntityComponent CurrentTarget { get; private set; }

        private void Awake()
        {
            _cameraProvider = CamerasController.Instance;
            if (_cameraProvider != null)
                _cameraProvider.OnActiveCameraChanged += HandleCameraChanged;

            _camera = Camera.main;

            if (_camera == null && debugMode)
                Debug.LogWarning("PlayerAimingComponent: Camera.main is null on Awake!");
        }

        private void Start()
        {
            _groundPlane = new Plane(Vector3.up, 0f);
            _aimInterfaces.AddRange(GetComponents<IAim>());
        }

        private void OnDestroy()
        {
            if (_cameraProvider != null)
                _cameraProvider.OnActiveCameraChanged -= HandleCameraChanged;
        }

        private void HandleCameraChanged(Camera newCamera)
        {
            _camera = newCamera;
            if (debugMode)
                Debug.Log($"PlayerAimingComponent: Camera changed to {(newCamera != null ? newCamera.name : "null")}");
        }

        private void Update()
        {
            if (Paused || _killed) return;

            if (_camera == null)
            {
                if (debugMode)
                    Debug.LogWarning("PlayerAimingComponent: No active camera!");
                return;
            }

            DoAiming();
        }

        private void DoAiming()
        {
            _groundPlane.distance = -(transform.position.y + aimOffset);
            Vector3 mousePosition = Input.mousePosition;
            Ray ray = _camera.ScreenPointToRay(mousePosition);

            // --- НАЧАЛО ОТЛАДКИ ---
            if (debugMode)
            {
                // Жёлтый луч от камеры
                Debug.DrawRay(ray.origin, ray.direction * 100f, Color.yellow);
            }

            RaycastHit hit;
            bool hitEntity = Physics.Raycast(ray, out hit, Mathf.Infinity, entitiesLayerMask);

            if (hitEntity && hit.collider.TryGetComponent<BaseGameEntityComponent>(out var entity) && entity.Targetable)
            {
                Vector3 aimTarget = GetEntityAimPoint(entity);
                _desiredLookDirection = (aimTarget - transform.position).normalized;
                CurrentTarget = entity;

                if (debugMode)
                {
                    // Красный луч от персонажа к точке прицеливания на сущности
                    Debug.DrawLine(transform.position, aimTarget, Color.red, Time.deltaTime);
                    Debug.Log($"[AIM] Entity: {entity.name}, aimTarget: {aimTarget}, direction: {_desiredLookDirection}");
                }
            }
            else
            {
                CurrentTarget = null;

                if (debugMode && hitEntity)
                {
                    // Луч попал в коллайдер, но компонент не найден
                    Debug.LogWarning($"[AIM] Hit {hit.collider.name} but no BaseGameEntityComponent found!");
                }

                if (_groundPlane.Raycast(ray, out float distance))
                {
                    Vector3 hitPoint = ray.GetPoint(distance);
                    _desiredLookDirection = (hitPoint - transform.position).normalized;

                    if (debugMode)
                    {
                        // Зелёный луч от персонажа к точке на земле
                        Debug.DrawLine(transform.position, hitPoint, Color.green, Time.deltaTime);
                    }
                }
                else
                {
                    if (debugMode)
                        Debug.LogWarning("[AIM] Ray missed both entities and ground!");

                    _desiredLookDirection = transform.forward;
                }
            }

            // Применяем направление ко всем IAim
            if (_aimInterfaces.Count > 0)
            {
                foreach (var aim in _aimInterfaces)
                {
                    aim.AimDirection = _desiredLookDirection;
                }
            }
        }

        private Vector3 GetEntityAimPoint(BaseGameEntityComponent entity)
        {
            if (entity is IAimTarget aimTarget)
                return aimTarget.GetAimPoint();
            return entity.transform.position;
        }
    }
}