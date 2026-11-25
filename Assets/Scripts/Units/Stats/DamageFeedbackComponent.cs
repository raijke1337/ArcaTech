using System.Collections;
using System.Collections.Generic;
using Arcatech.Managers;
using Arcatech.Rendering;
using Arcatech.Usables.Effects;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Arcatech.Stats
{
    [DisallowMultipleComponent][RequireComponent(typeof(BaseGameEntityComponent))]
    public class DamageFeedbackComponent : ValidatedMonoBehaviour, IStatReceiver, IDamageDrawer
    {
        [SerializeField, Self] private BaseGameEntityComponent entity;
        [SerializeField ]private float HitstopDuration = 0.3f;

        [Header("Hitstop")]
        [SerializeField] private bool playHitstop = true;

        [Header("Damage Flash")]
        [SerializeField] private bool playDamageFlash = true;
        [SerializeField, Tooltip("Leave empty to use the project's DamageFlash material.")]
        private Material damageFlashMaterial;
        [SerializeField, Tooltip("Leave empty to use the mesh and skinned mesh renderers on this object and its children.")]
        private Renderer[] flashRenderers;
[Header("Floating numbers")]
        [SerializeField] bool drawDamageNumbers = true;
        [SerializeField] private bool drawHealingNumbers = true;
        
        
        private readonly Dictionary<Renderer, Material[]> _originalMaterials = new();
        private readonly Dictionary<Material, Material> _flashMaterials = new();
        private readonly List<Material> _materialSlots = new();
        private Material _activeFlashMaterial;
        private Coroutine _flashRoutine;

        public bool Invulnerable { get; set; }

        public bool ApplyInstantDelta(StatDelta delta, BaseGameEntityComponent source, EffectKey key)
        {
            if (!isActiveAndEnabled || Invulnerable ||
                delta.stat != ResourceStatType.Health || delta.target != StatTarget.Current || !(delta.amount < 0f))
                return false;

            bool playedHitstop = playHitstop && DamagePipeline.HitStopEnabled &&
                                 source != null && source.CompareTag("Player");
            if (playedHitstop)
                HitstopPlayer.Instance.PlayHitstop(HitstopDuration, 0.2f);

            bool playedFlash = playDamageFlash && StartDamageFlash();
            return playedHitstop || playedFlash;
        }

        private bool StartDamageFlash()
        {
            if (_flashRoutine != null)
            {
                StopCoroutine(_flashRoutine);
            }
            else
            {
                if (damageFlashMaterial == null)
                    damageFlashMaterial = Resources.Load<Material>("Material/DamageFlash");
                if (damageFlashMaterial == null) return false;

                _activeFlashMaterial = damageFlashMaterial;
                var renderers = flashRenderers is { Length: > 0 }
                    ? flashRenderers
                    : GetComponentsInChildren<Renderer>(true);

                foreach (var renderer in renderers)
                {
                    if (renderer == null || renderer is not (MeshRenderer or SkinnedMeshRenderer) ||
                        _originalMaterials.ContainsKey(renderer))
                        continue;

                    var original = renderer.sharedMaterials;
                    if (original.Length == 0) continue;

                    _materialSlots.Clear();
                    bool changed = false;
                    foreach (var material in original)
                    {
                        var flashMaterial = GetFlashMaterial(material);
                        _materialSlots.Add(flashMaterial);
                        changed |= flashMaterial != material;
                    }

                    if (!changed) continue;
                    _originalMaterials.Add(renderer, original);
                    renderer.SetSharedMaterials(_materialSlots);
                }

                if (_originalMaterials.Count == 0)
                {
                    _materialSlots.Clear();
                    _flashMaterials.Clear();
                    _activeFlashMaterial = null;
                    return false;
                }
            }

            _flashRoutine = StartCoroutine(RestoreAfterHitstopDuration());
            return true;
        }

        private Material GetFlashMaterial(Material original)
        {
            // Keep transparent accents, such as the spider's glow, on their original pass.
            if (original == null || original.renderQueue > (int)RenderQueue.GeometryLast)
                return original;

            if (_flashMaterials.TryGetValue(original, out var flash))
                return flash;

            flash = _activeFlashMaterial;
            if (original.shader == flash.shader && flash.HasProperty(ArcaShaderIds.DamageFlashStrength))
            {
                // Preserve the source maps, shader keywords, and per-material outline.
                var preset = flash;
                flash = new Material(original);
                flash.SetFloat(ArcaShaderIds.DamageFlashStrength, preset.GetFloat(ArcaShaderIds.DamageFlashStrength));
                flash.SetColor(ArcaShaderIds.DamageFlashColor, preset.GetColor(ArcaShaderIds.DamageFlashColor));
                flash.SetFloat(ArcaShaderIds.DamageFlashDetail, preset.GetFloat(ArcaShaderIds.DamageFlashDetail));
                flash.SetFloat(ArcaShaderIds.DamageFlashHalftone, preset.GetFloat(ArcaShaderIds.DamageFlashHalftone));
                flash.SetFloat(ArcaShaderIds.DamageFlashEmission, preset.GetFloat(ArcaShaderIds.DamageFlashEmission));
            }

            _flashMaterials.Add(original, flash);
            return flash;
        }

        private IEnumerator RestoreAfterHitstopDuration()
        {
            yield return new WaitForSecondsRealtime(HitstopDuration);

            RestoreMaterials();
        }

        private void OnDisable()
        {
            if (_flashRoutine == null) return;
            StopCoroutine(_flashRoutine);
            RestoreMaterials();
        }

        private void RestoreMaterials()
        {
            foreach (var entry in _originalMaterials)
            {
                var renderer = entry.Key;
                if (renderer == null) continue;

                renderer.GetSharedMaterials(_materialSlots);
                bool changed = false;
                for (int i = 0; i < Mathf.Min(_materialSlots.Count, entry.Value.Length); i++)
                {
                    var original = entry.Value[i];
                    if (ReferenceEquals(original, null) || !_flashMaterials.TryGetValue(original, out var flash) ||
                        flash == original || _materialSlots[i] != flash)
                        continue;

                    _materialSlots[i] = original;
                    changed = true;
                }

                if (changed)
                    renderer.SetSharedMaterials(_materialSlots);
            }

            _materialSlots.Clear();
            _originalMaterials.Clear();
            foreach (var flash in _flashMaterials.Values)
                if (flash != _activeFlashMaterial)
                    Destroy(flash);
            _flashMaterials.Clear();
            _activeFlashMaterial = null;
            _flashRoutine = null;
        }
        
        public void DrawResourceChange(float amount, bool isDamage, float? durationOverride, 
            ResourceStatType type)
        {
            if (entity == null || entity.EffectSpawn == null || type != ResourceStatType.Health ) return;

            if (drawDamageNumbers != isDamage) return; 
            
            if (GameInterfaceManager.Instance == null)
            {
                Debug.LogWarning("DamageDrawerComponent: GameInterfaceManager.Instance not available.");
                return;
            }

            GameInterfaceManager.Instance.ShowFloatingNumber(
                amount,
                entity.EffectSpawn.position,
                isDamage,
                durationOverride
            );
        }
    }
}
