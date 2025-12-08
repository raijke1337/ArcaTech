
using Arcatech.Items;
using ECM2;
using NUnit.Framework.Constraints;
using UnityEngine;

namespace Arcatech.Units.Control
{

    [RequireComponent(typeof(EntityStateMachineComponent))]
    public class MovementControllerWithAiming : Character, IMove, IPausableComponent, IJump, IAim, IUnitCommandValidator
    {
        
        public bool CanMove { get; set; }
        public bool UseRootMotion { get => useRootMotion; set => useRootMotion = value; }
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
        
        public bool CanAim { get; set; }  = true;
        
        public Vector3 AimPosition { get; set; }

        protected override void CustomRotationMode(float deltaTime)
        {
            if (CanAim) RotateTowards(AimPosition,deltaTime);
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
            base.OnEnable();
            fmI = Animator.StringToHash(fm);
            smI = Animator.StringToHash(sm);
            vmI = Animator.StringToHash(vm);
            drI = Animator.StringToHash(dr);
            vI = Animator.StringToHash(fV);
        }


        private Vector2 lastDotVector;
        private void Animate()
        {
            animator.SetFloat(vI, GetVelocity().magnitude);
            // Dot product of two vectors determines how much they are pointing in the same direction.
            // If the vectors are normalized (transform.forward and right are)
            // then the value will be between -1 and +1.
        
            var fwd = transform.forward;
            var right = transform.right;
        
            if (MovementVector != Vector3.zero)
            {           
        
                
                var x = Vector3.Dot(right, Vector3.Normalize(GetVelocity()));
                var z = Vector3.Dot(fwd, Vector3.Normalize(GetVelocity()));
           
                lastDotVector.x = x;
                lastDotVector.y = z;
            
        
                animator.SetFloat(fmI, z);
                animator.SetFloat(smI,x);
                
                isStandingRotating = false;
                animator.ResetTrigger(drI);
            }
            else
            {
                animator.SetFloat(fmI, 0f, dampTime: 0.25f, deltaTime: Time.deltaTime);
                animator.SetFloat(smI, 0f, dampTime: 0.25f, deltaTime: Time.deltaTime);
        
                var crossY = (Mathf.Abs(Vector3.Cross(fwd, AimPosition).y));
        
                if  (crossY > minCrossYToRotate && GetCharacterMovement().isGrounded)
                {
                    animator.SetTrigger(drI);
                    isStandingRotating = true;
                }
                if (crossY <= 0.01f) // finished rotation
                {
                    isStandingRotating = false;
                }
            }
            animator.SetFloat(vmI, GetVelocity().y);

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
    }
}