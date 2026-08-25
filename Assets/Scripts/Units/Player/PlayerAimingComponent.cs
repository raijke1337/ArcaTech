using System.Collections.Generic;
using Arcatech.Managers;
using UnityEngine;

namespace Arcatech.Units.Control
{
    public enum AimMode
    {
        Free,
        TargetLocked
    }

    [RequireComponent(typeof(TargetSelector))]
    public sealed class PlayerAimingComponent :
        MonoBehaviour,
        IPausableComponent,
        IKillableComponent
    {
        [Header("Aim")]
        [SerializeField] private float aimOffset;
        [SerializeField] private LayerMask targetMask;
        [SerializeField, Min(0f)] private float searchRadius = 15f;
        [SerializeField, Range(0f, 180f)] private float searchAngle = 45f;

        [Header("Gamepad")]
        [SerializeField, Range(0f, 1f)] private float gamepadAimDeadZone = 0.2f;
        [SerializeField, Min(0f)] private float targetSwitchCooldown = 0.25f;

        [Header("Debug")]
        [SerializeField] private bool logTargetChanges = true;
        [SerializeField] private bool logAimWarnings = true;

        private readonly List<IAim> _aimInterfaces = new();

        private TargetSelector _targetSelector;
        private PlayerInputGateway _inputGateway;
        private Camera _camera;
        private Plane _groundPlane;

        private Vector3 _desiredLookDirection;
        private float _nextTargetSwitchTime;
        private bool _killed;

        public bool Paused { get; set; }

        public AimMode Mode { get; private set; } = AimMode.Free;

        public BaseGameEntityComponent CurrentTarget { get; private set; }

        private void Awake()
        {

            targetMask = LayerMask.GetMask(DataManager.GameRules.ValidHitsLayer);
            _targetSelector = GetComponent<TargetSelector>();
            _groundPlane = new Plane(Vector3.up, 0f);
            _camera = Camera.main;

            // Важно: компонент, реализующий IAim, может быть на дочернем объекте.
            _aimInterfaces.AddRange(GetComponentsInChildren<IAim>(true));

            if (_aimInterfaces.Count == 0 && logAimWarnings)
            {
                Debug.LogWarning(
                    $"[{nameof(PlayerAimingComponent)}] " +
                    $"На '{name}' не найден ни один компонент, " +
                    $"реализующий {nameof(IAim)}. " +
                    $"Персонаж не сможет поворачиваться.",
                    this);
            }
        }

        public void Initialize(PlayerInputGateway inputGateway)
        {
            if (_inputGateway != null)
                _inputGateway.AimChanged -= OnAimChanged;

            _inputGateway = inputGateway;

            if (_inputGateway != null)
                _inputGateway.AimChanged += OnAimChanged;
        }

        private void OnDestroy()
        {
            if (_inputGateway != null)
                _inputGateway.AimChanged -= OnAimChanged;
        }

        private void Update()
        {
            if (Paused || _killed)
                return;

            // Зафиксированная цель имеет абсолютный приоритет.
            // Поэтому персонаж продолжает смотреть на неё и во время бега.
            if (CurrentTarget != null)
            {
                if (!IsValidTarget(CurrentTarget))
                {
                    ClearTarget("Цель стала невалидной");
                    return;
                }

                _desiredLookDirection = GetPlanarDirection(
                    CurrentTarget.EffectSpawn.position - transform.position);

                ApplyAimDirection();
            }
        }

        private void OnAimChanged(Vector2 input)
        {
            if (Paused || _killed)
                return;

            if (_inputGateway != null && _inputGateway.IsGamepad)
                HandleGamepadAim(input);
            else
                HandleMouseAim(input);

            ApplyAimDirection();
        }

        private void HandleMouseAim(Vector2 mousePosition)
        {
            Camera camera = GetCamera();

            if (camera == null)
            {
                Debug.LogWarning(
                    $"[{nameof(PlayerAimingComponent)}] Камера не найдена.",
                    this);

                return;
            }

            _groundPlane.distance = -(transform.position.y + aimOffset);

            Ray ray = camera.ScreenPointToRay(mousePosition);


            if (Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    Mathf.Infinity,
                    targetMask,
                    QueryTriggerInteraction.Ignore))
            {
                var entity = hit.collider.GetComponentInParent<BaseGameEntityComponent>();

                if (IsValidTarget(entity))
                {
                    SetTarget(entity, "Выбрана мышью");

                    _desiredLookDirection = GetPlanarDirection(
                        entity.EffectSpawn.position - transform.position);

                    return;
                }
            }

            // Нет валидной цели под курсором:
            // свободно смотрим в точку, на которую указывает курсор.
            ClearTarget("Под курсором нет валидной цели");

            if (!_groundPlane.Raycast(ray, out float distance))
                return;

            Vector3 aimPoint = ray.GetPoint(distance);

            _desiredLookDirection = GetPlanarDirection(
                aimPoint - transform.position);
        }

        private void HandleGamepadAim(Vector2 stick)
        {
            float deadZoneSqr = gamepadAimDeadZone * gamepadAimDeadZone;

            if (stick.sqrMagnitude < deadZoneSqr)
                return;

            Vector3 stickWorldDirection = ConvertStickToWorldDirection(stick);

            BaseGameEntityComponent targetInStickDirection =
                _targetSelector.FindClosestTarget(
                    transform.position,
                    stickWorldDirection,
                    transform,
                    targetMask,
                    searchRadius,
                    searchAngle);

            // Если нет зафиксированной цели:
            // персонаж смотрит по направлению стика.
            if (CurrentTarget == null)
            {
                if (targetInStickDirection != null)
                {
                    SetTarget(
                        targetInStickDirection,
                        "Выбрана геймпадом");
                }
                else
                {
                    Mode = AimMode.Free;
                    _desiredLookDirection = stickWorldDirection;
                }

                return;
            }

            // Если цель уже зафиксирована, а в направлении стика нет другой:
            // сохраняем lock-on и продолжаем смотреть на текущую цель.
            if (targetInStickDirection == null ||
                targetInStickDirection == CurrentTarget)
            {
                return;
            }

            // Переключаем цель только после cooldown.
            if (Time.time < _nextTargetSwitchTime)
                return;

            SetTarget(
                targetInStickDirection,
                "Переключена геймпадом");

            _nextTargetSwitchTime = Time.time + targetSwitchCooldown;
        }

        private Vector3 ConvertStickToWorldDirection(Vector2 stick)
        {
            Camera camera = GetCamera();

            if (camera == null)
                return GetPlanarDirection(transform.forward);

            Vector3 cameraForward = camera.transform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            Vector3 cameraRight = camera.transform.right;
            cameraRight.y = 0f;
            cameraRight.Normalize();

            Vector3 worldDirection =
                cameraRight * stick.x +
                cameraForward * stick.y;

            return GetPlanarDirection(worldDirection);
        }

        private Camera GetCamera()
        {
            if (_camera == null)
                _camera = Camera.main;

            return _camera;
        }

        private void SetTarget(
            BaseGameEntityComponent target,
            string reason)
        {
            if (!IsValidTarget(target))
            {
                ClearTarget("Попытка выбрать невалидную цель");
                return;
            }

            if (CurrentTarget == target)
                return;

            CurrentTarget = target;
            Mode = AimMode.TargetLocked;

            _desiredLookDirection = GetPlanarDirection(
                target.EffectSpawn.position - transform.position);

            NotifyGameInterface(target);
            LogTarget(reason);
        }

        private void ClearTarget(string reason)
        {
            if (CurrentTarget == null && Mode == AimMode.Free)
                return;

            string previousTargetName = CurrentTarget != null
                ? CurrentTarget.name
                : "null";

            CurrentTarget = null;
            Mode = AimMode.Free;

            NotifyGameInterface(null);

            if (logTargetChanges)
            {
                // Debug.Log(
                //     $"[{nameof(PlayerAimingComponent)}] " +
                //     $"Цель снята. Предыдущая цель: '{previousTargetName}'. " +
                //     $"Причина: {reason}.",
                //     this);
            }
        }

        private void ApplyAimDirection()
        {
            if (_desiredLookDirection.sqrMagnitude < 0.0001f)
                return;

            foreach (IAim aim in _aimInterfaces)
            {
                if (aim == null)
                    continue;

                if (!aim.CanAim)
                    continue;

                aim.AimDirection = _desiredLookDirection;
            }
        }

        private void NotifyGameInterface(BaseGameEntityComponent target)
        {
            if (GameInterfaceManager.Instance != null)
                GameInterfaceManager.Instance.LockOnTarget(target);
        }

        private static Vector3 GetPlanarDirection(Vector3 direction)
        {
            direction.y = 0f;

            return direction.sqrMagnitude > 0.0001f
                ? direction.normalized
                : Vector3.zero;
        }

        private bool IsValidTarget(BaseGameEntityComponent target)
        {
            if (target == null ||
                target.EffectSpawn == null ||
                !target.gameObject.activeInHierarchy ||
                !target.Targetable)
            {
                return false;
            }

            // Сам игрок и его дочерние объекты никогда не могут быть целью.
            if (target.transform.IsChildOf(transform))
                return false;

            // Дополнительное исключение объектов с Player tag.
            if (target.CompareTag("Player"))
                return false;

            Vector3 offset =
                target.EffectSpawn.position - transform.position;

            offset.y = 0f;

            return offset.sqrMagnitude <= searchRadius * searchRadius;
        }

        private void LogTarget(string reason)
        {
            if (!logTargetChanges)
                return;

            string targetName = CurrentTarget != null
                ? CurrentTarget.name
                : "null";

            Debug.Log(
                $"[{nameof(PlayerAimingComponent)}] " +
                $"Текущая цель: '{targetName}'. " +
                $"Режим: {Mode}. " +
                $"Причина: {reason}.",
                this);
        }

        public void SetKilled(
            IKillerComponent component,
            bool value)
        {
            _killed = value;

            if (_killed)
                ClearTarget("Персонаж погиб");
        }
    }
}