using System;
using System.Collections.Generic;
using Arcatech.Units;
using Arcatech.Units.Control;
using ECM.Common;
using ECM.Controllers;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;

namespace Arcatech.Units.Control
{

    [RequireComponent(typeof(EntityStateMachineComponent))]
    public class MovementControllerWithAiming : BaseCharacterController, IPlayerMovementController,IPausableComponent,IKillableComponent, IUnitStateProvider
    {

        private EntityStateMachineComponent _stateMachine;
        [SerializeField] private SerializedUnitState JumpState;
        private UnitState _jump;

        public bool CanAim { get; set; }  = true;
        public bool CanMove { get; set; } = true;
        
        public Vector3 MovementVector
        {
            get => moveDirection;
            set
            {
                if (!CanMove)
                {
                    moveDirection = Vector3.zero;
                }
                else
                {
                    moveDirection = value;
                }
            }
        }

        public Vector3 AimPosition { get; set; }

        public bool JumpCommand { get; set; } = false;

        public bool Paused
        {
            get => pause;
            set => pause = value;
        }

        public bool Killed { get; set; } = false;
        
        
        //private float referenceMagnitude;
        private float airTime;
        private bool landOK;
        
        
        // for animator hashes
        readonly string fm = "ForwardMove";
        readonly string sm = "SideMove";
        private readonly string vm = "VerticalMove";
        readonly string im ="isMoving";
        readonly string trigger = "AdvanceState";
        private readonly string at = "AirTime";
        private readonly string dr = "DoStandingRotation";

        private int fmI;
        int smI;
        private int vmI;
        int imI;
        int triggerI;
        private int atI;
        private int drI;

        private bool isStandingRotating;

        [Header("animator setting")] [SerializeField]
        private float _minCrossYToRotate = 0.15f;
        private void Start()
        {
            _stateMachine =  GetComponent<EntityStateMachineComponent>();
            _jump = JumpState.DeserializeState(_stateMachine,_stateMachine.GetMainEntity.transform);
            fmI = Animator.StringToHash(fm);
            smI = Animator.StringToHash(sm);
            vmI = Animator.StringToHash(vm);
            imI = Animator.StringToHash(im);
            triggerI = Animator.StringToHash(trigger);
            atI = Animator.StringToHash(at);
            drI = Animator.StringToHash(dr);
        }

        protected override void HandleInput()
        {
            jump = JumpCommand;
           // referenceMagnitude = movement.velocity.magnitude;
        }
        protected override void UpdateRotation()
        {
            if (!CanAim) return;
            RotateTowards(AimPosition);
        }
        protected override void Animate()
        {
            // Dot product of two vectors determines how much they are pointing in the same direction.
            // If the vectors are normalized (transform.forward and right are)
            // then the value will be between -1 and +1.
            
            SetMovementValuesInAnimator();
            
            if (_isJumping && movement.wasGrounded)
            {
                _stateMachine.ForceUnitState(_jump);
            }
            HandleAirTime();
            
            if (!isGrounded)
            {
                airTime+=Time.deltaTime;
                
            }
           // animator.SetFloat("Rotation", Vector3.Cross(fwd, AimPosition).y);
          // set by aiming now 

            if (!movement.isGrounded)
            {
                landOK = false;
                airTime = 0;
            }
            if (movement.isGrounded)
            {
                if (!movement.wasGrounded || !landOK)
                {
                    animator.SetTrigger(triggerI);
                    landOK = true; 
                }
            }
            animator.SetFloat(atI, airTime);
        }

        private void HandleAirTime()
        {
            
        }
        
        private void SetMovementValuesInAnimator()
        {
            var fwd = transform.forward;
            var right = transform.right;

            if (MovementVector != Vector3.zero)
            {           

                Vector2 dot;
                var x = Vector3.Dot(right, Vector3.Normalize(movement.cachedRigidbody.linearVelocity));
                var z = Vector3.Dot(fwd, Vector3.Normalize(movement.cachedRigidbody.linearVelocity));
               
                dot.x = x;
                dot.y = z;
                

                animator.SetFloat(fmI, z);
                animator.SetFloat(smI,x);
                animator.SetBool(imI,true);
                isStandingRotating = false;
                animator.ResetTrigger(drI);
            }
            else
            {
                animator.SetFloat(fmI, 0);
                animator.SetFloat(smI, 0);
                animator.SetBool(imI, false);

                var crossY = (Mathf.Abs(Vector3.Cross(fwd, AimPosition).y));

                if  (crossY > _minCrossYToRotate && movement.isGrounded)
                {
                    animator.SetTrigger(drI);
                    isStandingRotating = true;
                }
                if (crossY <= 0.01f) // finished rotation
                {
                    isStandingRotating = false;
                }
            }
            animator.SetFloat(vmI, movement.cachedRigidbody.linearVelocity.y);
        }
        public override void Update()
        { // pause is handled internally
            if (Killed) return;
            base.Update();
        }

        public override void FixedUpdate()
        {
            if (Killed) return;
            base.FixedUpdate();
        }

        public IEnumerable<SerializedUnitState> GetStates { get; }
    }
}