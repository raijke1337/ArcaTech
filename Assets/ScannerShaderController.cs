using System.Collections;
using UnityEngine;

namespace Arcatech.Shaders
{
    public class ScannerShaderController : MonoBehaviour
    {

        [Header("Refs")]
        public Transform sonarSphere;     // sphere with URP/SonarSpherePulse
        public Renderer  sonarRenderer;   // optional: set color/intensity at runtime

        [Header("Pulse")]
        public float pulseInterval = 2.0f;     // seconds between pings
        public float maxRadius     = 20.0f;    // meters - range
        public float ringWidth     = 0.6f;     // meters (shared with outline)
        AnimationCurve radiusEase = AnimationCurve.Linear(0,0,1,1);

        [Header("Sync")] 
        public string colorProp = "_Color";
        public string originProp  = "_SonarOriginWS";
        public string radiusProp  = "_SonarRadius";
        public string widthProp   = "_SonarWidth";

        float t0;


        public void DrawScan(float range)
        {
            t0 = Time.time;
            if (sonarSphere == null) return;
        }
        
        void Update()
        {
            float t = (Time.time - t0) / Mathf.Max(0.0001f, pulseInterval);
            if (t >= 1.0f)
            {
                // restart pulse
                t0 = Time.time;
                t  = 0f;
            }

            float eased = radiusEase.Evaluate(t);
            float radius = Mathf.Lerp(0f, maxRadius, eased);

            // Scale the sphere so that its radius in world matches 'radius'
            // Unit sphere has radius 0.5 if using default Unity sphere, but many meshes are 0.5 or 1.0.
            // Assuming a standard Unity Sphere (radius 0.5), scale = radius / 0.5 = radius * 2.
            float sphereScale = radius * 2f;
            sonarSphere.localScale = new Vector3(sphereScale, sphereScale, sphereScale);
            sonarSphere.position = transform.position;

            // Share globals for the outline shader
            Shader.SetGlobalVector(originProp, transform.position);
            Shader.SetGlobalFloat(radiusProp, radius);
            Shader.SetGlobalFloat(widthProp, ringWidth);
        }
    
    }
}