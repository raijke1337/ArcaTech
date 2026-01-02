using KBCore.Refs;
using UnityEngine;

namespace Arcatech
{
    [RequireComponent(typeof(BaseGameEntityComponent))]
    public class SimpleEntityShadowComponent : MonoBehaviour
    {
       // [SerializeField,Self] BaseGameEntityComponent _entity;
        
        [Header("Shadow Settings")]
    [SerializeField] private float shadowSize = 1f;
    [SerializeField] private float shadowOpacity = 0.5f;
    [SerializeField] private Vector3 shadowOffset = new Vector3(0, -0.1f, 0);
    [SerializeField] private Color shadowColor = Color.black;
    [SerializeField] private bool followParent = true;
    
    // Shadow variables
    private GameObject shadowObject;
    private SpriteRenderer shadowRenderer;
    private Transform parentTransform;

    
    [Space,Header("Ground Detection")]
    [SerializeField] private bool useGroundDetection = true;
  //  [SerializeField] private LayerMask groundLayerMask = 1; // Default layer
    [SerializeField] private float maxRaycastDistance = 10f;
    


    void Start()
    {
        parentTransform = transform;
        CreateShadow();
    }
    
    void CreateShadow()
    {
        // Create shadow GameObject
        shadowObject = new GameObject($"{name}_Shadow");
        shadowObject.transform.localEulerAngles = new Vector3(90, 0, 0);
        shadowObject.transform.SetParent(transform);
        
        // Add SpriteRenderer
        shadowRenderer = shadowObject.AddComponent<SpriteRenderer>();
        
        // Create a simple circle texture
        shadowRenderer.sprite = CreateCircleSprite();
        shadowRenderer.color = new Color(shadowColor.r, shadowColor.g, shadowColor.b, shadowOpacity);
        
        // Set sorting order to render behind everything
        shadowRenderer.sortingOrder = -100;
        
        // Set initial scale
        shadowObject.transform.localScale = Vector3.one * shadowSize;
        //shadowObject.transform.localEulerAngles += new Vector3(90, 0, 0); // put it down flat
        
        UpdateShadowPosition();
    }
    
    void Update()
    {
        if (followParent && shadowObject != null)
        {
            UpdateShadowPosition();
        }
    }
    void UpdateShadowPosition()
    {
        if (shadowObject == null) return;
        
        Vector3 shadowPosition = parentTransform.position + shadowOffset;
        
        if (useGroundDetection)
        {
            // Raycast down to find ground
            RaycastHit hit;
            Vector3 rayStart = parentTransform.position;
            
            if (Physics.Raycast(rayStart, Vector3.down, out hit))
            {
                shadowPosition = hit.point + new Vector3(shadowOffset.x, 0.01f, shadowOffset.z);
                
                // Fade shadow based on distance from ground
                float distanceToGround = Vector3.Distance(parentTransform.position, hit.point);
                float fadeFactor = Mathf.Clamp01(1f - (distanceToGround / maxRaycastDistance));
                shadowRenderer.color = new Color(shadowColor.r, shadowColor.g, shadowColor.b, shadowOpacity * fadeFactor);
            }
            else
            {
                // No ground detected, hide shadow
                shadowRenderer.color = new Color(shadowColor.r, shadowColor.g, shadowColor.b, 0);
            }
        }
        
        shadowObject.transform.position = shadowPosition;
    }
    
    Sprite CreateCircleSprite()
    {
        // Create a simple circle texture
        int textureSize = 64;
        Texture2D texture = new Texture2D(textureSize, textureSize);
        
        Vector2 center = new Vector2(textureSize / 2f, textureSize / 2f);
        float radius = textureSize / 2f - 2f;
        
        for (int x = 0; x < textureSize; x++)
        {
            for (int y = 0; y < textureSize; y++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);
                
                if (distance <= radius)
                {
                    // Create soft edge
                    float alpha = 1f - Mathf.Clamp01((distance - radius + 4f) / 4f);
                    texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f));
    }
    
    // Public methods for runtime control
    public void SetShadowSize(float size)
    {
        shadowSize = size;
        if (shadowObject != null)
            shadowObject.transform.localScale = Vector3.one * shadowSize;
    }
    
    public void SetShadowOpacity(float opacity)
    {
        shadowOpacity = Mathf.Clamp01(opacity);
        if (shadowRenderer != null)
            shadowRenderer.color = new Color(shadowColor.r, shadowColor.g, shadowColor.b, shadowOpacity);
    }
    
    public void SetShadowColor(Color color)
    {
        shadowColor = color;
        if (shadowRenderer != null)
            shadowRenderer.color = new Color(shadowColor.r, shadowColor.g, shadowColor.b, shadowOpacity);
    }
    
    void OnDestroy()
    {
        if (shadowObject != null)
            DestroyImmediate(shadowObject);
    }
    
    void OnDrawGizmos()
    {
        if (useGroundDetection)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * maxRaycastDistance);
        }
    }
    }
}