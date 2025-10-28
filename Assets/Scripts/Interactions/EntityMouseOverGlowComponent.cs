
using System.Collections;
using System.Collections.Generic;
using Arcatech.EventBus;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Arcatech.Interactions
{
    
    [RequireComponent(typeof(BaseGameEntityComponent))]
    public class EntityMouseOverGlowComponent : ValidatedMonoBehaviour, ITargetable
    {
        [Self,SerializeField] BaseGameEntityComponent entity;
        private BaseEntityMouseOverEvent cachedEvent;
        public BaseGameEntityComponent GetEntity => entity;

        
        

        [Header("Outline Settings")]
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private Color outlineColor = Color.cyan;
    [Range(0.001f, 0.1f)] [SerializeField] private float outlineWidth = 0.02f;
    [SerializeField] private bool useGlow = true;
    [Range(0f, 5f)] [SerializeField] private float glowIntensity = 2f;
    
    [Header("Animation Settings")]
    [SerializeField] private bool animateAppearance = true;
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.15f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    
    private GameObject outlineParent;
    private Renderer[] originalRenderers;
    private OutlineRenderer[] outlineRenderers;
    private Coroutine currentAnimation;
    private bool isVisible = false;


    [SerializeField] private List<GlowOutlinePickerComponent> _glows;
    
    [System.Serializable]
    private class OutlineRenderer
    {
        public GameObject outlineObject;
        public Renderer renderer;
        public Material outlineMat;
    }
    
    void Start()
    {
        cachedEvent = new BaseEntityMouseOverEvent
        {
            Target = this
        };

        SetupOutlineSystem();
    }
    
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        cachedEvent.IsSelected = true;
        EventBus<BaseEntityMouseOverEvent>.Raise(cachedEvent);
        ShowOutline();
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        HideOutline();
        cachedEvent.IsSelected = false;
        EventBus<BaseEntityMouseOverEvent>.Raise(cachedEvent);
    }
    
    
    
    void SetupOutlineSystem()
    {
        // Get all renderers
        if (_glows == null)
        {
            _glows = new();
            _glows.AddRange(GetComponentsInChildren<GlowOutlinePickerComponent>());
        }
        originalRenderers = new Renderer[_glows.Count];
        for (int i = 0; i < _glows.Count; i++)
        {
            originalRenderers[i] = _glows[i].GetRenderer;
        }
        
        CreateOutlineObjects();
        SetOutlineVisibility(false);
    }
    
    void CreateOutlineObjects()
    {
        // Create parent for outline objects
        outlineParent = new GameObject($"{gameObject.name}_OutlineParent");
        outlineParent.transform.SetParent(transform);
        outlineParent.transform.localPosition = Vector3.zero;
        outlineParent.transform.localRotation = Quaternion.identity;
        outlineParent.transform.localScale = Vector3.one;
        
        outlineRenderers = new OutlineRenderer[originalRenderers.Length];
        
        for (int i = 0; i < originalRenderers.Length; i++)
        {
            CreateOutlineForRenderer(originalRenderers[i], i);
        }
    }
    
    void CreateOutlineForRenderer(Renderer originalRenderer, int index)
    {
        outlineRenderers[index] = new OutlineRenderer();
        
        // Create outline object
        GameObject outlineObj = new GameObject($"{originalRenderer.name}_Outline");
        outlineObj.transform.SetParent(outlineParent.transform);
        
        // Match transform
        outlineObj.transform.position = originalRenderer.transform.position;
        outlineObj.transform.rotation = originalRenderer.transform.rotation;
        outlineObj.transform.localScale = originalRenderer.transform.localScale * (1f + outlineWidth);
        
        // Copy mesh components
        if (originalRenderer is MeshRenderer meshRenderer)
        {
            MeshFilter originalMeshFilter = originalRenderer.GetComponent<MeshFilter>();
            if (originalMeshFilter != null)
            {
                MeshFilter outlineMeshFilter = outlineObj.AddComponent<MeshFilter>();
                outlineMeshFilter.mesh = originalMeshFilter.mesh;
                
                MeshRenderer outlineMeshRenderer = outlineObj.AddComponent<MeshRenderer>();
                SetupOutlineMaterial(outlineMeshRenderer, index);
                
                outlineRenderers[index].outlineObject = outlineObj;
                outlineRenderers[index].renderer = outlineMeshRenderer;
            }
        }
        else if (originalRenderer is SkinnedMeshRenderer skinnedRenderer)
        {
            SkinnedMeshRenderer outlineSkinnedRenderer = outlineObj.AddComponent<SkinnedMeshRenderer>();
            outlineSkinnedRenderer.sharedMesh = skinnedRenderer.sharedMesh;
            outlineSkinnedRenderer.bones = skinnedRenderer.bones;
            outlineSkinnedRenderer.rootBone = skinnedRenderer.rootBone;
            
            SetupOutlineMaterial(outlineSkinnedRenderer, index);
            
            outlineRenderers[index].outlineObject = outlineObj;
            outlineRenderers[index].renderer = outlineSkinnedRenderer;
        }
        
        // Set sorting order to render behind original
        if (outlineRenderers[index].renderer != null)
        {
            outlineRenderers[index].renderer.sortingOrder = originalRenderer.sortingOrder - 1;
        }
    }
    
    void SetupOutlineMaterial(Renderer outlineRenderer, int index)
    {
        Material mat;
        
        if (outlineMaterial != null)
        {
            mat = new Material(outlineMaterial);
        }
        else
        {
            // Create default outline material
            mat = CreateDefaultOutlineMaterial();
        }
        
        // Set material properties
        mat.SetColor("_Color", outlineColor);
        if (mat.HasProperty("_OutlineColor"))
            mat.SetColor("_OutlineColor", outlineColor);
        if (mat.HasProperty("_EmissionColor"))
            mat.SetColor("_EmissionColor", outlineColor * glowIntensity);
        
        outlineRenderer.material = mat;
        outlineRenderers[index].outlineMat = mat;
    }
    
    Material CreateDefaultOutlineMaterial()
    {
        // Try to find URP/Lit shader first, fallback to Unlit
        Shader outlineShader = Shader.Find("Universal Render Pipeline/Lit");
        if (outlineShader == null)
            outlineShader = Shader.Find("Universal Render Pipeline/Unlit");
    
        Material mat = new Material(outlineShader);
    
        // Set rendering mode to transparent for proper blending
        mat.SetFloat("_Surface", 1); // 0 = Opaque, 1 = Transparent
        mat.SetFloat("_Blend", 0); // 0 = Alpha, 1 = Premultiply, 2 = Additive, 3 = Multiply
    
        // Enable transparency
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHABLEND_ON");
        mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    
        // Set proper face culling for outline effect
        mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Front); // Cull front faces to show outline
    
        // Enable emission if glow is enabled
        if (useGlow)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetFloat("_EmissionEnabled", 1f);
        }
    
        return mat;
    }

    
    public void ShowOutline()
    {
        if (isVisible) return;
        
        isVisible = true;
        
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
        
        if (animateAppearance)
            currentAnimation = StartCoroutine(AnimateOutline(true));
        else
            SetOutlineVisibility(true);
    }
    
    public void HideOutline()
    {
        if (!isVisible) return;
        
        isVisible = false;
        
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
        
        if (animateAppearance)
            currentAnimation = StartCoroutine(AnimateOutline(false));
        else
            SetOutlineVisibility(false);
    }
    
    IEnumerator AnimateOutline(bool fadeIn)
    {
        float duration = fadeIn ? fadeInDuration : fadeOutDuration;
        float startAlpha = fadeIn ? 0f : 1f;
        float targetAlpha = fadeIn ? 1f : 0f;
        
        SetOutlineVisibility(true); // Make sure renderers are active during animation
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float curveValue = fadeCurve.Evaluate(progress);
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, curveValue);
            
            SetOutlineAlpha(currentAlpha);
            yield return null;
        }
        
        SetOutlineAlpha(targetAlpha);
        
        if (!fadeIn)
            SetOutlineVisibility(false);
        
        currentAnimation = null;
    }
    
    void SetOutlineVisibility(bool visible)
    {
        if (outlineParent != null)
            outlineParent.SetActive(visible);
    }
    
    void SetOutlineAlpha(float alpha)
    {
        foreach (var outlineRenderer in outlineRenderers)
        {
            if (outlineRenderer?.outlineMat != null)
            {
                Color color = outlineRenderer.outlineMat.GetColor("_Color");
                color.a = alpha * outlineColor.a;
                outlineRenderer.outlineMat.SetColor("_Color", color);
                
                if (useGlow && outlineRenderer.outlineMat.HasProperty("_EmissionColor"))
                {
                    Color emissionColor = outlineColor * glowIntensity * alpha;
                    outlineRenderer.outlineMat.SetColor("_EmissionColor", emissionColor);
                }
            }
        }
    }
    
    // Public methods for runtime control
    public void SetOutlineColor(Color color)
    {
        outlineColor = color;
        foreach (var outlineRenderer in outlineRenderers)
        {
            if (outlineRenderer?.outlineMat != null)
            {
                outlineRenderer.outlineMat.SetColor("_Color", color);
                if (useGlow && outlineRenderer.outlineMat.HasProperty("_EmissionColor"))
                    outlineRenderer.outlineMat.SetColor("_EmissionColor", color * glowIntensity);
            }
        }
    }
    
    public void SetOutlineWidth(float width)
    {
        outlineWidth = width;
        foreach (var outlineRenderer in outlineRenderers)
        {
            if (outlineRenderer?.outlineObject != null)
            {
                Transform originalTransform = outlineRenderer.outlineObject.transform;
                originalTransform.localScale = Vector3.one * (1f + width);
            }
        }
    }
    
    public void SetGlowIntensity(float intensity)
    {
        glowIntensity = intensity;
        if (useGlow)
        {
            foreach (var outlineRenderer in outlineRenderers)
            {
                if (outlineRenderer?.outlineMat != null && outlineRenderer.outlineMat.HasProperty("_EmissionColor"))
                {
                    outlineRenderer.outlineMat.SetColor("_EmissionColor", outlineColor * intensity);
                }
            }
        }
    }
    
    void OnDestroy()
    {
        if (outlineParent != null)
            DestroyImmediate(outlineParent);
    }
    
    protected override void OnValidate()
    {
        base.OnValidate();
        if (!GetComponentInChildren<GlowOutlinePickerComponent>())
        {
            Debug.LogWarning($"{this} has a glow glow component but no glowing parts");
        }
        else
        {
            _glows = new();
            _glows.AddRange(GetComponentsInChildren<GlowOutlinePickerComponent>());
        }
        
        // Update outline in real-time when changing values in inspector
        if (Application.isPlaying && outlineRenderers != null)
        {
            SetOutlineColor(outlineColor);
            SetOutlineWidth(outlineWidth);
            SetGlowIntensity(glowIntensity);
        }
    }
    }

    
}