using Arcatech.Items;
using Arcatech.Level;
using Arcatech.Usables.Effects;
using ECM2;
using UnityEngine;

namespace Arcatech.Units.Control
{
    [RequireComponent(typeof(BaseGameEntityComponent))]
    public class MovementControllerWithAiming : Character, IMove, IPausableComponent, IJump, IAim, IUnitCommandValidator
    {
        private BaseGameEntityComponent _baseGameEntityComponent;
        private IModifierAggregator _agg;
        private float _startSpeed;
        private float _mult = 1f;

        public bool IsGamepadInput { get; set; } = false;

        #region aim

        public bool CanAim { get; set; } = true;

        public Vector3 AimDirection { get; set; }
        public bool HasLockedTarget { get; set; } = false;

        #endregion

        public float SpeedMultiplier
        {
            get => _mult;
            set
            {
                if (value.Equals(_mult)) return;
                _mult = value;
                maxWalkSpeed = _startSpeed * _mult;
            }
        }

        public bool CanMove { get; set; }

        public bool UseRootMotion
        {
            get => useRootMotion;
            set => useRootMotion = value;
        }

        public Vector3 MovementVector
        {
            get => GetMovementDirection();
            set => SetMovementDirection(!CanMove ? Vector3.zero : value);
        }

        public float ActualMovementVelocity => GetCharacterMovement().velocity.magnitude;

        public bool Paused
        {
            get => isPaused;
            set => Pause(value);
        }

        protected override void CustomRotationMode(float deltaTime)
        {
            if (UseRootMotion)
            {
                RotateWithRootMotion();
                return;
            }

            if (!CanAim)
                return;

            bool isMoving = MovementVector.sqrMagnitude > 0.0001f;
            bool rotateToMovement = !HasLockedTarget && IsGamepadInput && isMoving;

            Vector3 targetDirection = rotateToMovement
                ? MovementVector
                : AimDirection;

            RotateTowards(targetDirection, deltaTime);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            rotationMode = RotationMode.Custom;
        }

        protected override void OnCharacterMovementUpdated(float deltaTime)
        {
            base.OnCharacterMovementUpdated(deltaTime);
            Animate();
            if (_agg != null)
            {
                SpeedMultiplier = _agg.GetMultiplier(ModifierParam.MoveSpeed);
            }
        }

        #region Conveyor

        // Система отсчёта ленты: собственное движение считается относительно неё,
        // скорость ленты добавляется сверху.
        private Vector3 _beltFrame;

        private Vector3 _driftVelocity;
        private float _driftExpireTime;
        private bool _driftImpartOnExit;

        // кэш поиска ленты по коллайдеру под ногами
        private Collider _lastGroundCollider;
        private ConveyorSurface _lastConveyor;

        protected override void OnBeforeSimulationUpdate(float deltaTime)
        {
            base.OnBeforeSimulationUpdate(deltaTime);

            ConveyorSurface conveyor = GetConveyorUnderFeet();
            if (conveyor != null)
                ApplyMotion(conveyor.CreateMotionRequest());

            ResolveDrift();
        }

        private ConveyorSurface GetConveyorUnderFeet()
        {
            if (!IsWalking())
                return null;

            Collider ground = characterMovement.groundCollider;
            if (ground == null)
            {
                _lastGroundCollider = null;
                _lastConveyor = null;
                return null;
            }

            if (ground != _lastGroundCollider)
            {
                _lastGroundCollider = ground;
                _lastConveyor = ground.GetComponentInParent<ConveyorSurface>();
            }

            return _lastConveyor != null && _lastConveyor.isActiveAndEnabled ? _lastConveyor : null;
        }

        private void ApplyConveyorDrift(in MotionRequest request)
        {
            Vector3 v = request.Velocity;
            v.y = 0f;

            _driftVelocity = v;
            _driftImpartOnExit = request.ImpartOnExit;
            _driftExpireTime = Time.time + (request.Duration > 0f ? request.Duration : 0.1f);
        }

        private void ResolveDrift()
        {
            if (Time.time <= _driftExpireTime)
            {
                _beltFrame = _driftVelocity;                  // лента активна
            }
            else if (IsWalking() || !_driftImpartOnExit)
            {
                _beltFrame = Vector3.zero;                    // сошли на землю или инерцию не сохраняем
            }
            // иначе: в воздухе сохраняем скорость ленты до приземления
        }

        public override Vector3 CalcVelocity(Vector3 velocity, Vector3 desiredVelocity,
                                             float friction, bool isFluid, float deltaTime)
        {
            if (_beltFrame == Vector3.zero)
                return base.CalcVelocity(velocity, desiredVelocity, friction, isFluid, deltaTime);

            // Обычная логика ECM2 (разгон/торможение/maxSpeed) считается относительно ленты
            Vector3 relative = velocity - _beltFrame;
            relative = base.CalcVelocity(relative, desiredVelocity, friction, isFluid, deltaTime);
            return relative + _beltFrame;
        }

        /// <summary>Скорость персонажа без учёта ленты (для анимаций и т.п.).</summary>
        private Vector3 RelativeVelocity => characterMovement.velocity - _beltFrame;

        #endregion

        #region Animate

        // for animator hashes
        private readonly string fm = "ForwardMove";
        private readonly string sm = "SideMove";
        private readonly string vm = "VerticalMove";
        private readonly string dr = "DoStandingRotation";
        private readonly string linVeloc = "LinearVelocity";

        private int fmI;
        private int smI;
        private int vmI;
        private int drI;
        private int linVelH;

        private bool isStandingRotating;

        [Header("animator setting")] [SerializeField]
        private float minCrossYToRotate = 0.15f;

        protected override void OnEnable()
        {
            _baseGameEntityComponent = GetComponent<BaseGameEntityComponent>();
            if (_baseGameEntityComponent.TryGetComponent<EffectsReceiverComponent>(out var receiverComponent))
            {
                receiverComponent.TryGetModifierAggregator(out _agg);
            }

            base.OnEnable();
            fmI = Animator.StringToHash(fm);
            smI = Animator.StringToHash(sm);
            vmI = Animator.StringToHash(vm);
            drI = Animator.StringToHash(dr);

            linVelH = Animator.StringToHash(linVeloc);
            _startSpeed = maxWalkSpeed;

            _beltFrame = Vector3.zero;
            _driftVelocity = Vector3.zero;
            _driftExpireTime = 0f;
        }

        private void Animate()
        {
            UpdateMovementBlend();    // ForwardMove / SideMove
            UpdateLinearSpeed();      // LinearVelocity
            UpdateVerticalSpeed();    // VerticalMove
            UpdateStandingRotation(); // DoStandingRotation trigger
        }

        private void UpdateLinearSpeed()
        {
            // Скорость относительно ленты: стоя на ленте персонаж не должен «бежать на месте»
            Vector3 rel = RelativeVelocity;
            rel.y = 0f;
            animator.SetFloat(linVelH, rel.magnitude);
        }

        private void UpdateVerticalSpeed()
        {
            animator.SetFloat(vmI, characterMovement.velocity.y);
        }

        private void UpdateMovementBlend()
        {
            const float DAMP_TIME = 0.25f;
            bool isMoving = MovementVector.sqrMagnitude > 0.0001f;

            if (isMoving)
            {
                Vector3 rel = RelativeVelocity;
                rel.y = 0f;
                float relSpeed = rel.magnitude;

                if (relSpeed > 0.0001f)
                {
                    animator.SetFloat(fmI, Vector3.Dot(rel, transform.forward) / relSpeed);
                    animator.SetFloat(smI, Vector3.Dot(rel, transform.right) / relSpeed);
                }
                else
                {
                    animator.SetFloat(fmI, 0f);
                    animator.SetFloat(smI, 0f);
                }

                // Активное движение всегда «съедает» поворот на месте
                if (isStandingRotating)
                {
                    animator.ResetTrigger(drI);
                    isStandingRotating = false;
                }
            }
            else
            {
                animator.SetFloat(fmI, 0f, DAMP_TIME, Time.deltaTime);
                animator.SetFloat(smI, 0f, DAMP_TIME, Time.deltaTime);
            }
        }

        private void UpdateStandingRotation()
        {
            // Условия старта поворота на месте:
            // 1. Стоим на месте (UpdateMovementBlend это уже проверил)
            // 2. На земле
            // 3. Отклонение прицела от форварда превышает порог
            if (!ShouldRotateWhileStanding()) return;

            float signedAngle = ComputeSignedAngleToAim();
            if (ShouldRotateWhileStanding())
            {
                animator.SetTrigger(drI);
                isStandingRotating = true;
            }
            if (signedAngle <= 0.01f)
                isStandingRotating = false;
        }

        private float ComputeSignedAngleToAim()
        {
            Vector3 flatAim = AimDirection;
            flatAim.y = 0f;

            if (flatAim.sqrMagnitude < 0.0001f) return 0f;

            return Vector3.SignedAngle(GetForwardVector(), flatAim.normalized, Vector3.up);
        }

        private bool ShouldRotateWhileStanding()
        {
            if (!IsGrounded) return false;
            if (MovementVector != Vector3.zero) return false;

            // AimDirection — единичный вектор; угол между forward и направлением прицела
            Vector3 flatAim = AimDirection; flatAim.y = 0;
            float signedAngle = Vector3.SignedAngle(GetForwardVector(), flatAim, Vector3.up);
            return Mathf.Abs(signedAngle) > minCrossYToRotate;
        }

        #endregion

        public bool CanDoUnitCommand(UnitCommand type, out string info)
        {
            info = "Movement ctrl jump:";
            switch (type.Type)
            {
                case UnitActionType.Jump:
                    bool canJump = CanJump();
                    info += canJump ? "OK" : "Cannot jump now";
                    return canJump;
                default:
                    info += "OK";
                    return true;
            }
        }

        protected override void OnLanded(Vector3 landingVelocity)
        {
            base.OnLanded(landingVelocity);
            StopJumping();
        }

        public bool JumpCommand
        {
            get => IsJumping();
            set
            {
                if (value && CanJump() && !Paused)
                {
                    Jump();
                }
            }
        }

        #region dodge/pushback

        [Header("Impulse Settings")]

        [Tooltip("Сколько секунд снимать привязку к земле после импульса, чтобы додж отрывал от пола.")]
        [SerializeField] private float _impulseGroundConstraintPause = 0.15f;

        /// <summary>
        /// Единая точка для отталкиваний, рывков и сноса (скорость в м/с уже посчитана вызывающим).
        /// Knockback  - заменяет боковую скорость (резкий откидывающий удар).
        /// Dash       - добавляется к текущей боковой скорости (бегущий юнит тормозит трением, а не телепортируется).
        /// Conveyor   - не импульс: задаёт систему отсчёта ленты, запрос нужно слать каждый тик.
        /// </summary>
        public void ApplyMotion(in MotionRequest request)
        {
            if (Paused) return;

            if (request.Kind == MotionKind.Conveyor)
            {
                ApplyConveyorDrift(request);
                return;
            }

            Vector3 v = request.Velocity;
            bool isDash = request.Kind == MotionKind.Dash;
            if (isDash) v.y = 0f;
            if (v.sqrMagnitude < 0.0001f) return;

            bool hasVertical = v.y > 0.01f;

            LaunchCharacter(
                launchVelocity:           v,
                overrideVerticalVelocity: hasVertical,      // без подброса гравитацию и вертикаль не трогаем
                overrideLateralVelocity:  !isDash);         // knockback перебивает боковую инерцию, dash складывается с ней

            PauseGroundConstraint(_impulseGroundConstraintPause);
        }

        #endregion
    }
}