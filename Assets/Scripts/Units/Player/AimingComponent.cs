using Arcatech.EventBus;
using Arcatech.Managers;
using Arcatech.Triggers;
using Arcatech.UI;
using Cinemachine;
using Cinemachine.Utility;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;

namespace Arcatech.Units.Inputs
{
    public class AimingComponent : MonoBehaviour, IManagedController
    {
        #region setup
        private Plane _aimPlane;

        [SerializeField, Range(0.1f, 3f)] float targetingSphereRadius = 1f;
        [SerializeField] float targetingUpdateFreq = 0.1f;

        ITargetable currentTgt;
        Collider[] checkColliders = new Collider[20];
        #endregion

        #region public properties
        Vector3 _target;
        float distanceToTarget;
        float _dotProduct;
        float _rotationToTarget;
        RaycastHit hit;
        public ITargetable Target => currentTgt;

        CountDownTimer resetTargetTimer;


        CinemachineBrain _br;
        public float GetDotProduct
        { get { return _dotProduct; } }

        public Vector3 GetLookTarget
        {
            get { return _target; }
        }
        public float GetDistanceToTarget
        {
            get
            {
                return distanceToTarget;
            }
        }
        /// <summary>
        /// positive = clockwise, negatve = CCW
        /// </summary>
        public float GetRotationToTarget
        {
            get => _rotationToTarget;
        }
        public Vector3 GetNormalizedDirectionToTaget
        {
            get
            {
                var heading = (_target - transform.position).normalized;
                return heading;
            }
        }
        public Vector3 GetDirectionToTarget
        {
            get
            {
                return _target - transform.position;
            }
        }
        public RaycastHit GetCurrentRaycastHit { get { return hit; } }
        #endregion


        private float prevY;
        float planeY = 0f;
        CountDownTimer targetUpdate;

        #region managed
        public void StartController()
        {
            Debug.Log("start aiming");
            _aimPlane = new Plane(Vector3.down, planeY);

            _target = transform.forward;
            _br = GetComponent<CinemachineBrain>();


            targetUpdate = new CountDownTimer(targetingUpdateFreq);
            resetTargetTimer = new CountDownTimer(1f); //todo
            targetUpdate.Start();
            resetTargetTimer.Start();
        }
        public void FixedControllerUpdate(float fixedDelta)
        {

        }

        public void ControllerUpdate(float delta)
        {
            // update aim plane position
            if (transform.position.y != prevY)
            {
                _aimPlane.Translate(Vector2.down * (transform.position.y - prevY));
                prevY = transform.position.y;
            }

            // aim at plane
            ////raycast at plane
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
           // Ray r = _br.OutputCamera.ScreenPointToRay(Input.mousePosition);
            
            _aimPlane.Raycast(r, out float rayDist);
            _target = r.GetPoint(rayDist);
            var vectorToTarget = _target - transform.position;
            // new
            vectorToTarget.ProjectOntoPlane(_aimPlane.normal);

            distanceToTarget = vectorToTarget.magnitude;
            _dotProduct = Vector3.Dot(transform.forward, GetNormalizedDirectionToTaget);
            _rotationToTarget = Vector3.Cross(transform.forward, GetNormalizedDirectionToTaget).y;


            //if (currentTgt is IInteractible)
            //{
            //    GameUIManager.Instance.SetCursor(CursorType.Item);
            //    if (currentTgt is BaseEntity)
            //    {
            //        GameUIManager.Instance.SetCursor(CursorType.EnemyTarget);
            //    }
            //}
            //else
            //{
            //    GameUIManager.Instance.SetCursor(CursorType.Explore);
            //}


            targetUpdate.Tick(delta);
            resetTargetTimer.Tick(delta);
            if (targetUpdate.IsReady)
            {
                CheckTargetables(_target);
                targetUpdate.Reset();
                targetUpdate.Start();
            }

        }

        void CheckTargetables(Vector3 target)
        {
            if (target == null) return; 

            if (Physics.OverlapSphereNonAlloc(target, targetingSphereRadius, checkColliders, LayerMask.NameToLayer("Ground")) > 0) // dont check ground layer objects
            {
                for (int i = 0; i < checkColliders.Length; i++)
                {
                    if (checkColliders[i] == null) break;
                    if (checkColliders[i].TryGetComponent<ITargetable>(out var component))
                    {
                        currentTgt = component;
                        break;
                    }
                }
            }
            else
            {
                    currentTgt = null;
            }

            EventBus<PlayerTargetUpdateEvent>.Raise(new PlayerTargetUpdateEvent(currentTgt));
        }

        public void StopController()
        {

        }
        #endregion

        private void OnDrawGizmos()
        {
            if (_target != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(_target, targetingSphereRadius);
            }
        }

        public bool CheckInteractive (out IInteractible item)
        {
            item = null;

            var hits = Physics.OverlapSphere(_target, targetingSphereRadius);
            if (hits.Length > 0)
            {
                for (int i = 0; i < hits.Length; i++)
                {
                    if (checkColliders[i] == null) break;
                    if (checkColliders[i].gameObject.layer.Equals(LayerMask.NameToLayer("Ground")) ) break;

                    if (checkColliders[i].TryGetComponent<IInteractible>(out var component))
                    {
                        item = component;
                        return true;
                    }
                }
                return false;
            }
           return false;
        }

    }
}