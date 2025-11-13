using System;
using System.Collections.Generic;
using Arcatech.Items;
using Arcatech.Units;
using Arcatech.Units.Control;
using ECM.Common;
using ECM.Controllers;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace Arcatech.Units.Control
{

    [RequireComponent(typeof(EntityStateMachineComponent))]
    public class MovementControllerWithAiming : BaseCharacterController, IPlayerMovementController,IPausableComponent,IKillableComponent
    {

        public bool IsGrounded => movement.isGrounded;
        public bool CanAim { get; set; }  = true;

        private bool move;

        public bool CanMove
        {
            get => move;
            set
            {
               // Debug.Log($"Can move: {value}");
                move = value;
                if (!value)
                {
                    moveDirection = Vector3.zero;
                    movement.cachedRigidbody.linearVelocity = Vector3.zero;
                   // Debug.Log($"set move vector to 0");
                }
            }
        }
    

        public Vector3 MovementVector
        {
            get => moveDirection;
            set => moveDirection = !CanMove ? Vector3.zero : value;
        }

        private Vector3 aim;

        public Vector3 AimPosition
        {
            get => aim;
            set
            {
                if (!CanAim) return;
                aim = value;
            }
        }

        public bool JumpCommand
        {
            get => _jump;
            set
            {
                _jump = value;
                if (!value) _canJump = true;
            }
        }

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
       // readonly string im ="isMoving";
        readonly string trigger = "AdvanceState";
        private readonly string at = "AirTime";
        private readonly string dr = "DoStandingRotation";
        private readonly string fV = "LinearVelocity";
        

        private int fmI;
        int smI;
        private int vmI;
       // int imI;
        int triggerI;
        private int atI;
        private int drI;
        private int vI;

        private bool isStandingRotating;

        [Header("animator setting")] [SerializeField]
        private float _minCrossYToRotate = 0.15f;
        private void Start()
        {
           // _stateMachine =  GetComponent<EntityStateMachineComponent>();
           // _jump = JumpState.Build();
            fmI = Animator.StringToHash(fm);
            smI = Animator.StringToHash(sm);
            vmI = Animator.StringToHash(vm);
        //    imI = Animator.StringToHash(im);
            triggerI = Animator.StringToHash(trigger);
            atI = Animator.StringToHash(at);
            drI = Animator.StringToHash(dr);
            vI = Animator.StringToHash(fV);
        }

        protected override void UpdateRotation()
        {
            if (!CanAim) return;
            RotateTowards(AimPosition);
        }
        protected override void Move()
        {
            if(!CanMove) return;
            base.Move();
        }
        protected override void Animate()
        {
            animator.SetFloat(vI,movement.cachedRigidbody.linearVelocity.magnitude);
            // Dot product of two vectors determines how much they are pointing in the same direction.
            // If the vectors are normalized (transform.forward and right are)
            // then the value will be between -1 and +1.

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
              //  animator.SetBool(imI,true);
                isStandingRotating = false;
                animator.ResetTrigger(drI);
            }
            else
            {
                animator.SetFloat(fmI, 0);
                animator.SetFloat(smI, 0);
               // animator.SetBool(imI, false);

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
        protected override void HandleInput()
        {
        }
        
    }
}