using System;
using System.Collections.Generic;
using Arcatech.Units.Control;
using com.cyborgAssets.inspectorButtonPro;
using Unity.Cinemachine;
using UnityEngine;
namespace Arcatech.Managers
{
    public class CamerasController : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera closeCam;

        [ProButton]
        public void ToggleCamera(float time = 5f)
        {
            closeCam.gameObject.SetActive(closeCam.gameObject.activeInHierarchy);
        }
    }
}

