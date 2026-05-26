using Arcatech.EventBus;
using Unity.Cinemachine;
using UnityEngine;
namespace Arcatech.Managers
{
    public class CamerasController : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera closeCam;

        private EventBinding<CameraEvent> _camBind;
        private void Start()
        {
            _camBind = new EventBinding<CameraEvent>(OnCameraEvent);
            EventBus<CameraEvent>.Register(_camBind);
        }
        private void OnCameraEvent(CameraEvent cameraEvent)
        {
            // 1) activate close up camera
            // 2) load the movement or orbiting settings into the camera
        }

        private void OnDisable()
        {
            EventBus<CameraEvent>.Deregister(_camBind);
        }
    }
    public struct CameraEvent : IEvent
    {
        // placeholder for global camera event calls
        // start / end
        // + movement path or settings
    }
}

