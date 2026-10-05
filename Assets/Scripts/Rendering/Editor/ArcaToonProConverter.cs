using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Arcatech.Rendering.EditorTools
{
    /// <summary>
    /// Переводит материалы Toon Pro ("Stylized Toon") на ARCA/CharacterToon.
    /// Материал меняется на месте - ссылки на него в префабах и сценах сохраняются.
    /// Поддерживает Undo (Ctrl+Z).
    ///
    /// Переносится: альбедо и тинт, нормали, AO, эмиссия, обводка (цвет, вкл/выкл,
    /// сглаженные нормали UV3), rim, cull. Рампы, штриховка, halftone, backlight
    /// Toon Pro не переносятся - их заменяет общая модель ARCA.
    /// Металл (_METAL) включается вручную на нужных материалах.
    /// </summary>
    public static class ArcaToonProConverter
    {
        private const string ToonProShader = "Stylized Toon";
        private const string TargetShader  = "ARCA/CharacterToon";

        [MenuItem("Arcatech/Rendering/Toon Pro -> CharacterToon (выделенные материалы)")]
        private static void ConvertSelected()
        {
            var mats = Selection.GetFiltered<Material>(SelectionMode.Assets | SelectionMode.DeepAssets);
            Convert(mats);
        }

        [MenuItem("Arcatech/Rendering/Toon Pro -> CharacterToon (все материалы проекта)")]
        private static void ConvertAll()
        {
            var mats = AssetDatabase.FindAssets("t:Material")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.StartsWith("Assets/"))
                .Select(AssetDatabase.LoadAssetAtPath<Material>)
                .Where(m => m != null && m.shader != null && m.shader.name == ToonProShader)
                .ToList();

            if (mats.Count == 0)
            {
                EditorUtility.DisplayDialog("Toon Pro", "Материалов на Toon Pro не найдено.", "OK");
                return;
            }

            string list = string.Join("\n", mats.Take(25).Select(m => "• " + m.name));
            if (mats.Count > 25) list += $"\n… и ещё {mats.Count - 25}";
            if (!EditorUtility.DisplayDialog("Toon Pro -> CharacterToon",
                    $"Будет переведено материалов: {mats.Count}\n\n{list}\n\nМожно отменить через Undo.",
                    "Перевести", "Отмена"))
                return;

            Convert(mats);
        }

        private static void Convert(IEnumerable<Material> materials)
        {
            Shader target = Shader.Find(TargetShader);
            if (target == null)
            {
                Debug.LogError($"[ArcaToonProConverter] Шейдер '{TargetShader}' не найден или не скомпилировался.");
                return;
            }

            int converted = 0;
            foreach (var m in materials)
            {
                if (m == null || m.shader == null || m.shader.name != ToonProShader) continue;

                Undo.RecordObject(m, "Toon Pro -> CharacterToon");

                // --- читаем значения Toon Pro до смены шейдера ---
                Color   color        = GetColor(m, "_Color", Color.white);
                Texture mainTex      = GetTex(m, "_MainTex");
                Vector2 mainScale    = m.HasProperty("_MainTex") ? m.GetTextureScale("_MainTex") : Vector2.one;
                Vector2 mainOffset   = m.HasProperty("_MainTex") ? m.GetTextureOffset("_MainTex") : Vector2.zero;
                Texture bump         = GetTex(m, "_BumpMap");
                float   bumpScale    = GetFloat(m, "_NormalMapStrength", 1f);
                Texture occ          = GetTex(m, "_OcclusionMap");
                float   occStrength  = GetFloat(m, "_OcclusionStrength", 1f);
                bool    useEmission  = GetFloat(m, "_UseEmission", 0f) > 0.5f;
                Color   emissionCol  = GetColor(m, "_EmissionColor", Color.black);
                Texture emissionMap  = GetTex(m, "_EmissionMap");
                bool    useOutline   = m.IsKeywordEnabled("_USEOUTLINE_ON");
                bool    uvBaked      = m.IsKeywordEnabled("_OUTLINETYPE_UVBAKED");
                Color   outlineCol   = GetColor(m, "_OutlineColor", new Color(0.05f, 0.04f, 0.06f));
                bool    useRim       = m.IsKeywordEnabled("_USERIMLIGHT_ON");
                Color   rimCol       = GetColor(m, "_RimColor", Color.white);
                float   cull         = GetFloat(m, "_Cull", 2f);

                // --- новый шейдер ---
                m.shader = target;

                m.SetColor("_BaseColor", color);
                m.SetTexture("_BaseMap", mainTex);
                m.SetTextureScale("_BaseMap", mainScale);
                m.SetTextureOffset("_BaseMap", mainOffset);
                m.SetTexture("_BumpMap", bump);
                m.SetFloat("_BumpScale", bumpScale);
                m.SetTexture("_OcclusionMap", occ);
                m.SetFloat("_OcclusionStrength", occ != null ? occStrength : 0f);

                m.SetColor("_EmissionColor", useEmission ? emissionCol : Color.black);
                m.SetTexture("_EmissionMap", useEmission ? emissionMap : null);

                m.SetColor("_OutlineColor", outlineCol);
                m.SetFloat("_OutlineWidthPx", useOutline ? 2f : 0f);
                m.SetFloat("_OutlineSmoothNormals", uvBaked ? 1f : 0f);
                SetKeyword(m, "_OUTLINE_SMOOTHNORMALS", uvBaked);

                // HDR-rim Toon Pro был очень ярким; приводим к спокойному уровню.
                Color rim = useRim ? ClampLdr(rimCol) * 0.5f : new Color(0.35f, 0.35f, 0.35f);
                rim.a = 1f;
                m.SetColor("_RimColor", rim);

                m.SetFloat("_Cull", cull);
                m.SetFloat("_Metal", 0f);
                SetKeyword(m, "_METAL", false);

                EditorUtility.SetDirty(m);
                converted++;
                Debug.Log($"[ArcaToonProConverter] {m.name}: переведён на {TargetShader}", m);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[ArcaToonProConverter] Готово, переведено материалов: {converted}.");
        }

        private static Color GetColor(Material m, string name, Color fallback) =>
            m.HasProperty(name) ? m.GetColor(name) : fallback;

        private static float GetFloat(Material m, string name, float fallback) =>
            m.HasProperty(name) ? m.GetFloat(name) : fallback;

        private static Texture GetTex(Material m, string name) =>
            m.HasProperty(name) ? m.GetTexture(name) : null;

        private static Color ClampLdr(Color c)
        {
            float max = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
            return max > 1f ? c / max : c;
        }

        private static void SetKeyword(Material m, string keyword, bool on)
        {
            if (on) m.EnableKeyword(keyword);
            else m.DisableKeyword(keyword);
        }
    }
}
