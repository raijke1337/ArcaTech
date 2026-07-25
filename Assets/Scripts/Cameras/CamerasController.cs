using System;
using Arcatech.Managers;
using Unity.Cinemachine;
using UnityEngine;

[DefaultExecutionOrder(-500)]
public class CamerasController : GenericLazySingleton<CamerasController>
{
    [SerializeField] private CinemachineBrain _brain;
    
    private Camera _fallbackCamera;
    private Camera _previousActiveCamera;

    /// <summary>Событие вызывается при смене активной камеры (в том числе в первом кадре).</summary>
    public event Action<Camera> OnActiveCameraChanged;

    public Camera ActiveCamera { get; private set; }
    
    private void Awake()
    {
        if (_brain == null)
            _brain = GetComponentInChildren<CinemachineBrain>();
        
        _fallbackCamera = Camera.main;
        _previousActiveCamera = null;
    }
    
    private void LateUpdate()
    {
        Camera newActiveCamera = _brain != null && _brain.OutputCamera != null 
            ? _brain.OutputCamera 
            : _fallbackCamera;

        if (newActiveCamera != _previousActiveCamera)
        {
            ActiveCamera = newActiveCamera;
            _previousActiveCamera = ActiveCamera;
            OnActiveCameraChanged?.Invoke(ActiveCamera);
        }
    }
}