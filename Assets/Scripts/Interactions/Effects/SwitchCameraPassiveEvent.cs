using System.Collections;
using Arcatech.SaveSystem;
using Unity.Cinemachine;
using UnityEngine;

namespace Arcatech.Interactions
{
    public class SwitchCameraPassiveEvent : InteractionEffect
    {
        [SerializeField] private CinemachineCamera cameraToActivate;
        [SerializeField] private float cameraTime = 2f;


        IEnumerator CameraRoutine()
        {
            cameraToActivate.gameObject.SetActive(true);
            yield return new WaitForSeconds(cameraTime);
            cameraToActivate.gameObject.SetActive(false);
        }

        public override void Play(InteractionContext ctx)
        {
            StartCoroutine(CameraRoutine());
        }

    }
}