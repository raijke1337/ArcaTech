using Arcatech.Items;
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


        #region aim
        
        public bool CanAim { get; set; } = true;

        public Vector3 AimDirection { get; set; }
        
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
            if (CanAim) RotateTowards(AimDirection, deltaTime);
            base.CustomRotationMode(deltaTime);
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
        }

        private void Animate()
        {
            UpdateMovementBlend();   // ForwardMove / SideMove
            UpdateLinearSpeed();     // LinearVelocity
            UpdateVerticalSpeed();   // VerticalMove
            UpdateStandingRotation();// DoStandingRotation trigger
        }
        private void UpdateLinearSpeed()
        {
            // Используется в Speed-параметре бленд-дерева (Walk → Run)
            animator.SetFloat(linVelH, characterMovement.speed);
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
                if (characterMovement.speed > 0.0001f)
                {
                    animator.SetFloat(fmI,
                        characterMovement.forwardSpeed / characterMovement.speed);
                    animator.SetFloat(smI,
                        characterMovement.sidewaysSpeed / characterMovement.speed);
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
                animator.SetFloat(smI,   0f, DAMP_TIME, Time.deltaTime);
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

        [Tooltip("Горизонтальная скорость (м/с), сообщаемая импульсом ±1 (додж игрока).")]
        [SerializeField] private float _impulseSpeed = 8f;

        [Tooltip("Сколько секунд снимать привязку к земле после импульса, чтобы додж отрывал от пола.")]
        [SerializeField] private float _impulseGroundConstraintPause = 0.15f;

        /// <summary>
        /// Knockback / отдача от внешнего источника (взрыв, удар босса) в мировых координатах.
        /// Заменяет текущую боковую скорость — knockback должен ощущаться резко и коммитить.
        /// </summary>
        public void ApplyImpulse(Vector3 impulse)
        {
            if (Paused || impulse.sqrMagnitude < 0.0001f) return;

            LaunchCharacter(
                launchVelocity:           impulse,
                overrideVerticalVelocity: false,  // гравитацию и текущую вертикаль не трогаем
                overrideLateralVelocity:  true);  // knockback перебивает боковую инерцию

            PauseGroundConstraint(_impulseGroundConstraintPause);
        }

        /// <summary>
        /// Додж игрока: импульс относительно moveDir игрока.
        /// -1 = полный назад, 0 = нет, +1 = полный вперёд.
        /// Добавляется к текущей боковой скорости — бегущий юнит тормозит трением, а не телепортируется.
        /// </summary>
        public void ApplyImpulse(float impulseRelative)
        {
            if (Paused) return;

            float t = Mathf.Clamp(impulseRelative, -1f, 1f);
            if (Mathf.Approximately(t, 0f)) return;

            Vector3 worldImpulse = MovementVector * (t * _impulseSpeed);

            LaunchCharacter(
                launchVelocity:           worldImpulse,
                overrideVerticalVelocity: false,
                overrideLateralVelocity:  false);  // додж складывается с инерцией

            PauseGroundConstraint(_impulseGroundConstraintPause);
        }
        #endregion
    }
}