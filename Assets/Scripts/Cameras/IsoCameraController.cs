using System;
using Arcatech.Units.Inputs;
using UnityEngine;
namespace Arcatech.Scenes.Cameras
{
    public class IsoCameraController : MonoBehaviour
    {
        private PlayerAimingComponent _playerAimingComponent;
        private Vector3 _cameraTargetPoint;

        [SerializeField] bool DebugMessage = false;
        [Header("Camera settings")]
        [SerializeField] private Camera _camera;
        [SerializeField] private Vector3 _desiredOffsetFromPlayer = Vector3.zero;
        [SerializeField, Range(0.1f, 3), Tooltip("How fast the camera moves")] private float _catchUpSpeed = 2f;
        [SerializeField,Range(1,5f),Tooltip("extra range when looking with mouse")] private float _lookDist = 1f;

        

        private void Start()
        {
            _camera = GetComponent<Camera>();      
        }

        public void Update()
        {
            if (_playerAimingComponent == null)
            {
                _playerAimingComponent = FindAnyObjectByType<PlayerAimingComponent>();
                transform.position = _playerAimingComponent.transform.position + _desiredOffsetFromPlayer;
            }
            else
            {
                if (_playerAimingComponent.GetDistanceToTarget < _lookDist)
                {

                    _cameraTargetPoint = _playerAimingComponent.GetLookTarget;
                }
                else
                {

                    _cameraTargetPoint = _playerAimingComponent.transform.position +
                                         _playerAimingComponent.GetNormalizedDirectionToTaget * _lookDist;

                }

                transform.position = Vector3.Slerp(transform.position, _cameraTargetPoint + _desiredOffsetFromPlayer,
                    Time.deltaTime * _catchUpSpeed);

            }


        }

    }


}

