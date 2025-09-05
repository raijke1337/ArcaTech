using ECM.Controllers;
using System.Collections;
using UnityEngine;

namespace Arcatech.Units
{
    public class DashJumpMovementController : BaseCharacterController
    {
        Rigidbody _rb;

        [Header("Aiming settings")]
        [SerializeField, Tooltip("If value is less, play rotation animation and rotate player")]
        protected float _minCrossYToRotate = 0.4f;
        public float AirTime { get => _jumpUngroundedTimer; }
        protected Vector3 desiredLookDirection;
        public void SetDesiredMoveDirection(Vector3 dir) => moveDirection = dir;
        public void SetDesiredLookDirection(Vector3 dir, bool force = false)
        {
            desiredLookDirection = dir.normalized;
            _forceRotation = force;
        }

        bool _forceRotation = false; // used when there is a target

        #region jump
        bool landOK = true;
        public void DoJump()
        {
            StartCoroutine(JumpToggle());
        }
        protected IEnumerator JumpToggle()
        {
            jump = true;
            yield return new WaitForSeconds(0.1f);
            jump = false;
        }
        #endregion
        #region dash
        // movement moved to dotween

        CountDownTimer dashTimer;
        Vector3 dodgeVector;
        float maxDodgeSpeed;
        public void DisableGroundingOnUnitImpulse(float speed, float distance)
        {
            float time = distance / speed;
            movement.DisableGrounding(time);
            // dodgeVector = transform.forward;
            // maxDodgeSpeed = Mathf.Abs(impulse);

            //// Debug.Log($"Start dodge str {impulse} over {time} direction {dodgeVector}");
            // dodgeVector *= impulse;

            //if (dashTimer == null || dashTimer.IsReady)
            //{
            //    //Debug.Log($"New dodge");
            //    dashTimer = new CountDownTimer(time);
            //    dashTimer.OnTimerStopped += OnDashFinish;
            //    dashTimer.Start();
            //}
            //else
            //{
            //    // Debug.Log($"Extend dodge");
            //    OnDashFinish();
            //    dashTimer.Reset(time);
            //    dashTimer.OnTimerStopped += OnDashFinish;
            //    dashTimer.Start();
            //}
        }
        //void DoDashing()
        //{
        //    //movement.Move(dodgeVector, maxDodgeSpeed);
        //   // Debug.Log($"Doing dodge dir {dodgeVector}");

        //    // cancel any vertical velocity while dashing on air (e.g. Cancel gravity)

        //    //if (!movement.isOnGround)
        //    //{
        //    //    movement.velocity = Vector3.ProjectOnPlane(movement.velocity, transform.up);
        //    //}

        //}
        //void OnDashFinish()
        //{
        // //   Debug.Log($"Dodge finished");
        //    dashTimer.OnTimerStopped -= OnDashFinish;
        //    // Cancel dash momentum, if not grounded, preserve gravity

        //    if (isGrounded)
        //        movement.velocity = Vector3.zero;
        //    else
        //        movement.velocity = Vector3.Project(movement.velocity, transform.up);
        //}

        #endregion
        bool isStandingRotating = false;
        protected override void Animate()
        {
            var fwd = transform.forward;
            var right = transform.right;

            // Dot product of two vectors determines how much they are pointing in the same direction.
            // If the vectors are normalized (transform.forward and right are)
            // then the value will be between -1 and +1.

            Vector2 dot;
            if (moveDirection.magnitude > 0f)
            {
                var x = Vector3.Dot(right, Vector3.Normalize(_rb.linearVelocity));
                var z = Vector3.Dot(fwd, Vector3.Normalize(_rb.linearVelocity));

                dot.x = x;
                dot.y = z;

                animator.SetFloat("ForwardMove", z);
                animator.SetFloat("SideMove", x);
                animator.SetBool("isMoving", true);
                isStandingRotating = false;
            }

            else
            {
                animator.SetFloat("ForwardMove", 0);
                animator.SetFloat("SideMove", 0);
                animator.SetBool("isMoving", false);

                var crossY = (Mathf.Abs(Vector3.Cross(fwd, desiredLookDirection).y));

                if  (crossY > _minCrossYToRotate && movement.isGrounded)
                {
                    animator.SetTrigger("DoStandingRotation");
                    isStandingRotating = true;
                }
                if (crossY <= 0.01f) // finished rotation
                {
                    isStandingRotating = false;
                }
            }
            animator.SetFloat("VerticalMove", _rb.linearVelocity.y);
            animator.SetFloat("Rotation", Vector3.Cross(fwd, desiredLookDirection).y);



            if (!movement.isGrounded)
            {
                landOK = false;
                animator.SetFloat("AirTime", AirTime);
            }
            if (movement.isGrounded)
            {
                if (!movement.wasGrounded || !landOK)
                {
                    animator.SetTrigger("Land");
                    landOK = true; 
                }
                animator.SetFloat("AirTime", AirTime);
            }

        }
        protected override void Move()
        {
            //if (dashTimer != null && dashTimer.IsRunning)
            //{
            //    DoDashing();
            //}
            //else
            //{
            //    base.Move();
            //}

            base.Move();
        }

        public override void Update()
        {
            dashTimer?.Tick(Time.deltaTime);
            base.Update();
        }

        protected override void HandleInput() { }

        // always rotate when moving
        protected override void UpdateRotation()
        {
            if (moveDirection.sqrMagnitude > 0f || isStandingRotating || _forceRotation)
            {
                RotateTowards(desiredLookDirection);
            }
        }
        public override void Awake()
        {
            base.Awake();
            _rb = GetComponent<Rigidbody>();
        }


        private void OnDrawGizmos()
        {
            if (moveDirection == null) return;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + moveDirection);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + desiredLookDirection);
        }
    }
}