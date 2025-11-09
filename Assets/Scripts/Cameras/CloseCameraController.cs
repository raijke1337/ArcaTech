using com.cyborgAssets.inspectorButtonPro;
using KBCore.Refs;
using Unity.Cinemachine;
using UnityEngine;

public class CloseCameraController : ValidatedMonoBehaviour
{
    [SerializeField,Self] CinemachineOrbitalFollow freeLook; // assign in inspector
    [Tooltip("Degrees per second of automatic yaw rotation")]
    public float orbitSpeed = 45f;
    [Tooltip("When true, the camera will orbit")]
    public bool orbitActive = false;
    
    void Update()
    {
        if (freeLook == null) return;

        if (orbitActive)
        {
            // Advance the FreeLook X axis (degrees per second)
            freeLook.HorizontalAxis.Value += orbitSpeed * Time.deltaTime;
            // Optional: keep value in [0,360) if you prefer
           // if (freeLook.RadialAxis.Value >= 360f) freeLook.RadialAxis.Value -= 360f;
        }
    }

    [ProButton]
    // Public methods to control activation (call from other scripts/inputs)
    public void ActivateOrbit()
    {
        orbitActive = true;
        // Optionally raise priority instead of enabling GameObject
        // freeLook.Priority = 20;
        // Or: freeLook.gameObject.SetActive(true);
    }
    [ProButton]
    public void DeactivateOrbit()
    {
        orbitActive = false;
        // Optionally lower priority
        // freeLook.Priority = 0;
        // Or: freeLook.gameObject.SetActive(false);
    }
}
