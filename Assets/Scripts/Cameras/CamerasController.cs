using System;
using System.Collections.Generic;
using Arcatech.Units.Control;
using com.cyborgAssets.inspectorButtonPro;
using Unity.Cinemachine;
using UnityEngine;
namespace Arcatech
{
    public class CamerasController : MonoBehaviour
    {
        #region obsolete
        // private PlayerAimingComponent _playerAimingComponent;
        // private Vector3 _cameraTargetPoint;
        //
        // [SerializeField] bool DebugMessage = false;
        // [Header("Camera settings")]
        // [SerializeField] private Camera _camera;
        // [SerializeField] private Vector3 _desiredOffsetFromPlayer = Vector3.zero;
        // [SerializeField, Range(0.1f, 3), Tooltip("How fast the camera moves")] private float _catchUpSpeed = 2f;
        // [SerializeField,Range(1,5f),Tooltip("extra range when looking with mouse")] private float _lookDist = 1f;
        //
        public void Update()
        {
            // old stuff

            // transform.position = Vector3.Slerp(transform.position, _cameraTargetPoint + _desiredOffsetFromPlayer,
            //        Time.deltaTime * _catchUpSpeed);

        }
        
        #endregion
        // switching to Cinemachine

        [ProButton]
        public void ToggleCamera()
        {
            
        }

        private List<CinemachineCamera> cameras;

        private void Awake()
        {
            cameras = new (GetComponentsInChildren<CinemachineCamera>());
        }
    }
}

