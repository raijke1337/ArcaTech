using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Units.Control
{
    public enum AimMode
    {
        Free,
        TargetLocked
    }

    public sealed class PlayerAimingComponent :
        MonoBehaviour,
        IPausableComponent,
        IKillableComponent
    {
        [Header("Aim")]
        [SerializeField] private float aimOffset;
        [SerializeField] private LayerMask entitiesLayerMask;
        [SerializeField] private LayerMask groundLayerMask;
        [SerializeField] private float targetSearchRadius = 15f;
        [SerializeField] private float targetSearchAngle = 45f;

        [Header("Gamepad")]
        [SerializeField] private float gamepadAimDeadZone = 0.2f;
        [SerializeField] private float targetSwitchCooldown = 0.25f;

        private readonly List<IAim> _aimInterfaces = new();

        private PlayerInputGateway _inputGateway;
        private Camera _camera;
        private Plane _groundPlane;

        private Vector3 _desiredLookDirection;

        private float _nextTargetSwitchTime;

        private bool _killed;

        public bool Paused { get; set; }

        public AimMode Mode { get; private set; } = AimMode.Free;

        public BaseGameEntityComponent CurrentTarget { get; private set; }

        public void Initialize(PlayerInputGateway inputGateway)
        {
            _inputGateway = inputGateway;

            _inputGateway.AimChanged += OnAimChanged;
        }

        private void Awake()
        {
            _groundPlane = new Plane(Vector3.up, 0f);

            _aimInterfaces.AddRange(
                GetComponents<IAim>());

            _camera = Camera.main;
        }

        private void OnDestroy()
        {
            if (_inputGateway != null)
                _inputGateway.AimChanged -= OnAimChanged;
        }

        private void OnAimChanged(Vector2 value)
        {
            if (Paused || _killed)
                return;

            if (_inputGateway.IsGamepad)
            {
                HandleGamepadAim(value);
            }
            else
            {
                HandleMouseAim(value);
            }

            ApplyAimDirection();
        }

        private void HandleMouseAim(Vector2 mousePosition)
        {
            if (_camera == null)
                _camera = Camera.main;

            if (_camera == null)
                return;

            _groundPlane.distance =
                -(transform.position.y + aimOffset);

            Ray ray = _camera.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(
                    ray,
                    out RaycastHit entityHit,
                    Mathf.Infinity,
                    entitiesLayerMask))
            {
                if (entityHit.collider.TryGetComponent(
                        out BaseGameEntityComponent entity) &&
                    entity.Targetable)
                {
                    SetTarget(entity);

                    Vector3 aimPoint = entity.EffectSpawn.position;

                    _desiredLookDirection =
                        (aimPoint - transform.position).normalized;

                    return;
                }
            }

            ClearTarget();

            if (_groundPlane.Raycast(ray, out float distance))
            {
                Vector3 point = ray.GetPoint(distance);

                _desiredLookDirection =
                    (point - transform.position).normalized;
            }
        }

        private void HandleGamepadAim(Vector2 stick)
        {
            if (stick.sqrMagnitude <
                gamepadAimDeadZone * gamepadAimDeadZone)
            {
                return;
            }

            Vector3 worldDirection = ConvertStickToWorldDirection(stick);

            BaseGameEntityComponent target =
                FindTargetInDirection(worldDirection);

            if (target != null)
            {
                SetTarget(target);
                Mode = AimMode.TargetLocked;

                _desiredLookDirection =
                    (target.EffectSpawn.position -
                     transform.position).normalized;
            }
            else
            {
                ClearTarget();

                Mode = AimMode.Free;
                _desiredLookDirection = worldDirection;
            }
        }

        private Vector3 ConvertStickToWorldDirection(Vector2 stick)
        {
            if (_camera == null)
                _camera = Camera.main;

            if (_camera == null)
                return transform.forward;

            Vector3 cameraForward = _camera.transform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            Vector3 cameraRight = _camera.transform.right;
            cameraRight.y = 0f;
            cameraRight.Normalize();

            Vector3 direction =
                cameraRight * stick.x +
                cameraForward * stick.y;

            direction.y = 0f;

            return direction.sqrMagnitude > 0.0001f
                ? direction.normalized
                : transform.forward;
        }

        private BaseGameEntityComponent FindTargetInDirection(
            Vector3 direction)
        {
            Collider[] hits = Physics.OverlapSphere(
                transform.position,
                targetSearchRadius,
                entitiesLayerMask);

            BaseGameEntityComponent bestTarget = null;
            float bestScore = float.MinValue;

            foreach (Collider hit in hits)
            {
                if (!hit.TryGetComponent(
                        out BaseGameEntityComponent entity))
                {
                    continue;
                }

                if (!entity.Targetable)
                    continue;

                Vector3 toTarget =
                    entity.EffectSpawn.position -
                    transform.position;

                toTarget.y = 0f;

                if (toTarget.sqrMagnitude < 0.0001f)
                    continue;

                Vector3 targetDirection = toTarget.normalized;

                float angle = Vector3.Angle(
                    direction,
                    targetDirection);

                if (angle > targetSearchAngle)
                    continue;

                float alignment = Vector3.Dot(
                    direction,
                    targetDirection);

                float distancePenalty = toTarget.magnitude;

                float score =
                    alignment * 10f -
                    distancePenalty;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = entity;
                }
            }

            return bestTarget;
        }

        private void SetTarget(
            BaseGameEntityComponent target)
        {
            CurrentTarget = target;
        }

        private void ClearTarget()
        {
            CurrentTarget = null;
        }

        private void ApplyAimDirection()
        {
            if (_desiredLookDirection.sqrMagnitude < 0.0001f)
                return;

            foreach (IAim aim in _aimInterfaces)
            {
                if (aim.CanAim)
                    aim.AimDirection = _desiredLookDirection;
            }
        }

        public void SetKilled(
            IKillerComponent component,
            bool value)
        {
            _killed = value;

            if (_killed)
            {
                ClearTarget();
            }
        }
    }
}