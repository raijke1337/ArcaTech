#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using Arcatech.Usables.Effects;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModifierAggregator))]
public class ModifierAggregatorInspector : Editor
{
    // ── Reflection ───────────────────────────────────────────────────────────
    private static readonly FieldInfo _fStacks =
        typeof(ModifierAggregator).GetField("_stacks", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly System.Type _stackEntryType =
        typeof(ModifierAggregator).GetNestedType("StackEntry", BindingFlags.NonPublic);

    private static readonly FieldInfo _seKey        = _stackEntryType?.GetField("key");
    private static readonly FieldInfo _seParam      = _stackEntryType?.GetField("param");
    private static readonly FieldInfo _seMultiplier = _stackEntryType?.GetField("multiplier");

    // ── UI State ─────────────────────────────────────────────────────────────
    private readonly Dictionary<ModifierParam, bool> _paramFoldouts = new();
    private bool _showRaw;

    // ── Стили ────────────────────────────────────────────────────────────────
    private GUIStyle _headerStyle;
    private GUIStyle _paramHeaderStyle;
    private GUIStyle _multiplierStyle;
    private GUIStyle _neutralBadge;
    private GUIStyle _boostBadge;
    private GUIStyle _penaltyBadge;
    private bool _stylesReady;

    // ── Цвета ────────────────────────────────────────────────────────────────
    private static readonly Color ColBoost   = new(0.4f, 1f,  0.4f, 1f);   // > 1 — зелёный
    private static readonly Color ColPenalty = new(1f,  0.35f, 0.35f, 1f); // < 1 — красный
    private static readonly Color ColNeutral = new(0.85f, 0.85f, 0.85f, 1f);
    private static readonly Color ColRowEven = new(0.22f, 0.22f, 0.25f, 0.4f);
    private static readonly Color ColRowOdd  = new(0.18f, 0.18f, 0.20f, 0.4f);

    private static readonly Dictionary<ModifierParam, Color> ParamColors = new()
    {
        { ModifierParam.MoveSpeed,      new Color(0.4f, 0.8f, 1f)  },
        { ModifierParam.OutgoingDamage, new Color(1f,  0.6f, 0.2f) },
        { ModifierParam.IncomingDamage, new Color(1f,  0.35f, 0.35f) },
    };

    public override bool RequiresConstantRepaint() => Application.isPlaying;

    // ────────────────────────────────────────────────────────────────────────
    public override void OnInspectorGUI()
    {
        InitStyles();

        var agg = (ModifierAggregator)target;

        DrawHeader();
        EditorGUILayout.Space(4);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Данные модификаторов доступны только в Play Mode.", MessageType.Info);
            return;
        }

        // ── Читаем данные через рефлексию ────────────────────────────────────
        var rawList = _fStacks.GetValue(agg) as System.Collections.IList;
        if (rawList == null || rawList.Count == 0)
        {
            EditorGUILayout.HelpBox("Нет активных стаков модификаторов.", MessageType.None);
            return;
        }

        var entries = ReadEntries(rawList);

        DrawSummaryBar(agg, entries);
        EditorGUILayout.Space(6);
        DrawParamSections(agg, entries);
        EditorGUILayout.Space(4);
        DrawRawTable(entries);
    }

    // ── Заголовок ────────────────────────────────────────────────────────────
    private void DrawHeader()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label("ModifierAggregator", _headerStyle, GUILayout.ExpandWidth(true));
            GUILayout.Label($"стаков: {GetRawCount()}", EditorStyles.miniLabel);
        }
    }

    private int GetRawCount()
    {
        var agg     = (ModifierAggregator)target;
        var rawList = _fStacks.GetValue(agg) as System.Collections.IList;
        return rawList?.Count ?? 0;
    }

    // ── Сводная строка по каждому параметру ──────────────────────────────────
    private void DrawSummaryBar(ModifierAggregator agg,
        List<(ModifierParam param, EffectKey key, float mult)> entries)
    {
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField("Итоговые множители", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);

            foreach (ModifierParam p in System.Enum.GetValues(typeof(ModifierParam)))
            {
                float product = agg.GetMultiplier(p);
                int   count   = 0;
                foreach (var e in entries) if (e.param == p) count++;

                using (new EditorGUILayout.HorizontalScope())
                {
                    // Цветная метка параметра
                    var prevContent = GUI.contentColor;
                    GUI.contentColor = ParamColors.TryGetValue(p, out var pc) ? pc : ColNeutral;
                    GUILayout.Label(p.ToString(), EditorStyles.boldLabel, GUILayout.Width(140));
                    GUI.contentColor = prevContent;

                    // Значение множителя
                    DrawMultiplierLabel(product);

                    GUILayout.FlexibleSpace();

                    // Количество стаков
                    if (count > 0)
                    {
                        var badgeStyle = count > 1 ? _boostBadge : _neutralBadge;
                        GUILayout.Label($"{count} stack{(count > 1 ? "s" : "")}", badgeStyle,
                            GUILayout.Width(60));
                    }
                    else
                    {
                        GUILayout.Label("—", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(60));
                    }
                }
            }
        }
    }

    // ── Секции по параметрам с раскрывающимися стаками ───────────────────────
    private void DrawParamSections(ModifierAggregator agg,
        List<(ModifierParam param, EffectKey key, float mult)> entries)
    {
        foreach (ModifierParam p in System.Enum.GetValues(typeof(ModifierParam)))
        {
            // Собираем стаки только этого параметра
            var group = new List<(ModifierParam param, EffectKey key, float mult)>();
            foreach (var e in entries) if (e.param == p) group.Add(e);
            if (group.Count == 0) continue;

            _paramFoldouts.TryGetValue(p, out bool open);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                // ── Заголовок секции ─────────────────────────────────────
                using (new EditorGUILayout.HorizontalScope())
                {
                    open = EditorGUILayout.Foldout(open, GUIContent.none, true);

                    var prevContent = GUI.contentColor;
                    GUI.contentColor = ParamColors.TryGetValue(p, out var pc) ? pc : ColNeutral;
                    GUILayout.Label(p.ToString(), _paramHeaderStyle, GUILayout.ExpandWidth(true));
                    GUI.contentColor = prevContent;

                    DrawMultiplierLabel(agg.GetMultiplier(p));
                    GUILayout.Label($"({group.Count})", EditorStyles.miniLabel, GUILayout.Width(28));
                }

                _paramFoldouts[p] = open;

                if (!open) continue;

                EditorGUI.indentLevel++;
                EditorGUILayout.Space(2);

                // ── Колонки ──────────────────────────────────────────────
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("EffectId",    EditorStyles.miniBoldLabel, GUILayout.Width(180));
                    GUILayout.Label("SourceId",    EditorStyles.miniBoldLabel, GUILayout.Width(100));
                    GUILayout.Label("Multiplier",  EditorStyles.miniBoldLabel, GUILayout.Width(80));
                    GUILayout.Label("Δ from 1",    EditorStyles.miniBoldLabel, GUILayout.Width(70));
                }

                DrawSeparator();

                // Группируем по EffectId для визуального разделения
                string lastEffectId = null;
                for (int i = 0; i < group.Count; i++)
                {
                    var e = group[i];

                    // Фон чередующихся строк
                    var rowRect = GUILayoutUtility.GetRect(0, 18, GUILayout.ExpandWidth(true));
                    EditorGUI.DrawRect(rowRect, (i % 2 == 0) ? ColRowEven : ColRowOdd);

                    // Разделитель при смене EffectId
                    if (lastEffectId != null && lastEffectId != e.key.EffectId)
                        DrawSeparator();
                    lastEffectId = e.key.EffectId;

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label(TruncateId(e.key.EffectId, 24), EditorStyles.miniLabel,
                            GUILayout.Width(180));
                        GUILayout.Label(TruncateId(e.key.SourceId, 12), EditorStyles.miniLabel,
                            GUILayout.Width(100));

                        // Множитель с цветом
                        var prevContent = GUI.contentColor;
                        GUI.contentColor = MultiplierColor(e.mult);
                        GUILayout.Label($"× {e.mult:F4}", EditorStyles.miniLabel, GUILayout.Width(80));
                        GUI.contentColor = prevContent;

                        // Дельта
                        float delta = e.mult - 1f;
                        GUI.contentColor = MultiplierColor(e.mult);
                        GUILayout.Label(delta >= 0 ? $"+{delta:P1}" : $"{delta:P1}",
                            EditorStyles.miniLabel, GUILayout.Width(70));
                        GUI.contentColor = prevContent;
                    }
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(2);
        }
    }

    // ── Сырая таблица всех стаков (опциональная) ─────────────────────────────
    private void DrawRawTable(List<(ModifierParam param, EffectKey key, float mult)> entries)
    {
        _showRaw = EditorGUILayout.Foldout(_showRaw, $"Raw стаки ({entries.Count})", true);
        if (!_showRaw) return;

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("#",          EditorStyles.miniBoldLabel, GUILayout.Width(24));
                GUILayout.Label("Param",      EditorStyles.miniBoldLabel, GUILayout.Width(120));
                GUILayout.Label("EffectId",   EditorStyles.miniBoldLabel, GUILayout.Width(160));
                GUILayout.Label("SourceId",   EditorStyles.miniBoldLabel, GUILayout.Width(100));
                GUILayout.Label("×",          EditorStyles.miniBoldLabel, GUILayout.Width(70));
            }

            DrawSeparator();

            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                var rowRect = GUILayoutUtility.GetRect(0, 18, GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(rowRect, (i % 2 == 0) ? ColRowEven : ColRowOdd);

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(i.ToString(),                   EditorStyles.miniLabel, GUILayout.Width(24));
                    GUILayout.Label(e.param.ToString(),             EditorStyles.miniLabel, GUILayout.Width(120));
                    GUILayout.Label(TruncateId(e.key.EffectId, 20), EditorStyles.miniLabel, GUILayout.Width(160));
                    GUILayout.Label(TruncateId(e.key.SourceId, 12), EditorStyles.miniLabel, GUILayout.Width(100));

                    var prev = GUI.contentColor;
                    GUI.contentColor = MultiplierColor(e.mult);
                    GUILayout.Label($"×{e.mult:F4}", EditorStyles.miniLabel, GUILayout.Width(70));
                    GUI.contentColor = prev;
                }
            }
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    private void DrawMultiplierLabel(float product)
    {
        var prev = GUI.contentColor;
        GUI.contentColor = MultiplierColor(product);

        string label = product >= 1f
            ? $"× {product:F3}  ▲ +{(product - 1f):P1}"
            : $"× {product:F3}  ▼ {(product - 1f):P1}";

        GUILayout.Label(label, _multiplierStyle, GUILayout.Width(160));
        GUI.contentColor = prev;
    }

    private static Color MultiplierColor(float v) =>
        Mathf.Approximately(v, 1f) ? ColNeutral : v > 1f ? ColBoost : ColPenalty;

    private static void DrawSeparator()
    {
        var rect = GUILayoutUtility.GetRect(0, 1, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.4f));
        GUILayout.Space(1);
    }

    private static string TruncateId(string id, int max) =>
        string.IsNullOrEmpty(id)  ? "—"
        : id.Length <= max        ? id
                                  : id.Substring(0, max - 1) + "…";

    private static List<(ModifierParam, EffectKey, float)>
        ReadEntries(System.Collections.IList rawList)
    {
        var result = new List<(ModifierParam, EffectKey, float)>(rawList.Count);
        foreach (var raw in rawList)
        {
            var param = (ModifierParam)_seParam.GetValue(raw);
            var key   = (EffectKey)_seKey.GetValue(raw);
            var mult  = (float)_seMultiplier.GetValue(raw);
            result.Add((param, key, mult));
        }
        return result;
    }

    // ── Стили ────────────────────────────────────────────────────────────────
    private void InitStyles()
    {
        if (_stylesReady) return;

        _headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize  = 13,
            alignment = TextAnchor.MiddleLeft
        };

        _paramHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize  = 11,
            alignment = TextAnchor.MiddleLeft
        };

        _multiplierStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize  = 11,
            alignment = TextAnchor.MiddleRight
        };

        _neutralBadge = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            normal    = { textColor = ColNeutral }
        };

        _boostBadge = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal    = { textColor = ColBoost }
        };

        _penaltyBadge = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal    = { textColor = ColPenalty }
        };

        _stylesReady = true;
    }
}
#endif