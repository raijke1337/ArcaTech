using Cinemachine.Utility;
using UnityEngine;

namespace Arcatech.Units.Inputs
{
    public class PlayerAimingComponent : MonoBehaviour
    {
        #region setup
        private Plane _aimPlane;

        [SerializeField, Range(0.1f, 3f)] float targetingSphereRadius = 1f;
        [SerializeField] float targetingUpdateFreq = 0.1f;

        Collider[] checkColliders = new Collider[20];
        #endregion


        float _dotProduct;
        float _rotationToTarget;
        RaycastHit hit;
        public ITargetable Target { get; }

        private CountDownTimer resetTargetTimer;

        #region public properties
        public float GetDotProduct => _dotProduct;

        public Vector3 GetLookTarget { get; private set; }

        public float GetDistanceToTarget { get; private set; }

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
                var heading = (GetLookTarget - transform.position).normalized;
                return heading;
            }
        }
        public Vector3 GetDirectionToTarget
        {
            get
            {
                return GetLookTarget - transform.position;
            }
        }
        #endregion


        private float prevY;
        float planeY = 0f;
        CountDownTimer targetUpdate;

        #region managed

        bool init = false;
        private void OnEnable()
        {
            
            _aimPlane = new Plane(Vector3.down, planeY);
            GetLookTarget = transform.forward;

            targetUpdate = new CountDownTimer(targetingUpdateFreq);
            resetTargetTimer = new CountDownTimer(1f); //todo
            targetUpdate.Start();
            resetTargetTimer.Start();
            init = true;
        }

        void Update()
        {
            if (!init) return;
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
            GetLookTarget = r.GetPoint(rayDist);
            var vectorToTarget = GetLookTarget - transform.position;
            // new
            vectorToTarget.ProjectOntoPlane(_aimPlane.normal);

            GetDistanceToTarget = vectorToTarget.magnitude;
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


            targetUpdate.Tick(Time.deltaTime);
            resetTargetTimer.Tick(Time.deltaTime);
            if (targetUpdate.IsReady)
            {
                //CheckTargetables(_target);
                targetUpdate.Reset();
                targetUpdate.Start();
            }
        }
        private void OnDisable()
        {
            init = false;
        }
        //void CheckTargetables(Vector3 target)
        //{
        //    if (target == null) return; 

        //    if (Physics.OverlapSphereNonAlloc(target, targetingSphereRadius, checkColliders, LayerMask.NameToLayer("Ground")) > 0) // dont check ground layer objects
        //    {
        //        for (int i = 0; i < checkColliders.Length; i++)
        //        {
        //            if (checkColliders[i] == null) return;
        //            if (checkColliders[i].TryGetComponent<ITargetable>(out var component))
        //            {
        //                currentTgt = component;
        //                break;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        currentTgt = null;
        //    }

        //    EventBus<PlayerTargetUpdateEvent>.Raise(new PlayerTargetUpdateEvent(currentTgt));
        //}

        #endregion

        // this is now done through event system  
        


    }
}