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

        public bool CanAim { get; set; } = true;

        public Vector3 AimPosition { get; set; }

        protected override void CustomRotationMode(float deltaTime)
        {
            if (CanAim) RotateTowards(AimPosition, deltaTime);
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
        private readonly string fV = "LinearVelocity";

        private int fmI;
        private int smI;
        private int vmI;
        private int drI;
        private int vI;

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

            vI = Animator.StringToHash(fV);
            _startSpeed = maxWalkSpeed;
        }


        private Vector2 lastDotVector;

        private void Animate()
        {
            // LinearVelocity — модуль скорости (м/с), нужен для бленд-деревьев
            animator.SetFloat(vI, characterMovement.speed);

            if (MovementVector != Vector3.zero)
            {
                // forwardSpeed/sidewaysSpeed — это «проекция velocity на оси» в м/с.
                // Делим на speed, чтобы получить нормализованный -1..1, как и было.
                float speed = characterMovement.speed;
                if (speed > 0.0001f)
                {
                    float fwd = characterMovement.forwardSpeed  / speed;
                    float side = characterMovement.sidewaysSpeed / speed;
                    animator.SetFloat(fmI, fwd);
                    animator.SetFloat(smI, side);
                }
                else
                {
                    animator.SetFloat(fmI, 0f);
                    animator.SetFloat(smI, 0f);
                }

                isStandingRotating = false;
                animator.ResetTrigger(drI);
            }
            else
            {
                animator.SetFloat(fmI, 0f, dampTime: 0.25f, deltaTime: Time.deltaTime);
                animator.SetFloat(smI, 0f, dampTime: 0.25f, deltaTime: Time.deltaTime);

                Vector3 fwd = GetForwardVector();
                var crossY = Mathf.Abs(Vector3.Cross(fwd, AimPosition).y);

                if (crossY > minCrossYToRotate && IsGrounded)
                {
                    animator.SetTrigger(drI);
                    isStandingRotating = true;
                }
                if (crossY <= 0.01f)
                    isStandingRotating = false;
            }

            animator.SetFloat(vmI, characterMovement.velocity.y);
        }

        #endregion

        public bool CanDoUnitCommand(UnitActionType type, out string info)
        {
            info = "Movement ctrl jump:";
            switch (type)
            {
                case UnitActionType.Jump:
                    info += CanJump();
                    return CanJump();
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
                if (value) Jump();
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