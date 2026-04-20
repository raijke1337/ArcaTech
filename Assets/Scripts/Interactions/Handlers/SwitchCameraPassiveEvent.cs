using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace Arcatech.Interactions
{
    public class SwitchCameraPassiveEvent : PassiveInteractionHandlerBase
    {
        [SerializeField] private CinemachineCamera cameraToActivate;
        [SerializeField] private float cameraTime = 2f;
        public override void OnInteractorEnter(IInteractor interactor)
        {
            StartCoroutine(CameraRoutine());
        }

        public override void OnInteractorExit(IInteractor interactor)
        {
        }

        IEnumerator CameraRoutine()
        {
            cameraToActivate.gameObject.SetActive(true);
            yield return new WaitForSeconds(cameraTime);
            cameraToActivate.gameObject.SetActive(false);
        }
    }
}