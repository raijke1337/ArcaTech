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
    private static readonly FieldInfo _seCounting   = _stackEntryType?.GetField("counting");
    private static readonly FieldInfo _seMaxStacks  = _stackEntryType?.GetField("maxStacks");

    // ── Тип записи для внутреннего использования ─────────────────────────────
    private readonly struct EntryData
    {
        public readonly ModifierParam         Param;
        public readonly EffectKey             Key;
        public readonly float                 Mult;
        public readonly ModifierStackCounting Counting;
        public readonly int                   MaxStacks;

        public EntryData(ModifierParam p, EffectKey k, float m,
                         ModifierStackCounting c, int ms)
        {
            Param     = p;
            Key       = k;
            Mult      = m;
            Counting  = c;
            MaxStacks = ms;
        }
    }

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
    private GUIStyle _capReachedStyle;
    private bool _stylesReady;

    // ── Цвета ────────────────────────────────────────────────────────────────
    private static readonly Color ColBoost      = new(0.4f,  1f,   0.4f, 1f);
    private static readonly Color ColPenalty    = new(1f,   0.35f, 0.35f, 1f);
    private static readonly Color ColNeutral    = new(0.85f, 0.85f, 0.85f, 1f);
    private static readonly Color ColCapReached = new(1f,   0.6f,  0.1f, 1f);
    private static readonly Color ColRowEven    = new(0.22f, 0.22f, 0.25f, 0.4f);
    private static readonly Color ColRowOdd     = new(0.18f, 0.18f, 0.20f, 0.4f);
    private static readonly Color ColSeparator  = new(0.5f,  0.5f,  0.5f, 0.4f);

    private static readonly Dictionary<ModifierParam, Color> ParamColors = new()
    {
        { ModifierParam.MoveSpeed,      new Color(0.4f, 0.8f, 1f)    },
        { ModifierParam.OutgoingDamage, new Color(1f,  0.6f, 0.2f)   },
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
            EditorGUILayout.HelpBox("Данные модификаторов доступны только в Play Mode.",
                MessageType.Info);
            return;
        }

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
        var rawList = _fStacks.GetValue((ModifierAggregator)target) as System.Collections.IList;
        return rawList?.Count ?? 0;
    }

    // ── Сводная строка ───────────────────────────────────────────────────────
    private void DrawSummaryBar(ModifierAggregator agg, List<EntryData> entries)
    {
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField("Итоговые множители", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);

            foreach (ModifierParam p in System.Enum.GetValues(typeof(ModifierParam)))
            {
                float product   = agg.GetMultiplier(p);
                int   stacksNow = 0;
                int   maxCap    = 0;

                foreach (var e in entries)
                {
                    if (e.Param != p) continue;
                    stacksNow++;
                    if (e.MaxStacks > maxCap) maxCap = e.MaxStacks;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    // Цветная метка параметра
                    var prevContent = GUI.contentColor;
                    GUI.contentColor = ParamColors.TryGetValue(p, out var pc) ? pc : ColNeutral;
                    GUILayout.Label(p.ToString(), EditorStyles.boldLabel, GUILayout.Width(140));
                    GUI.contentColor = prevContent;

                    DrawMultiplierLabel(product);
                    GUILayout.FlexibleSpace();
                    DrawStackCapBadge(stacksNow, maxCap);
                }
            }
        }
    }

    // ── Секции по параметрам ─────────────────────────────────────────────────
    private void DrawParamSections(ModifierAggregator agg, List<EntryData> entries)
    {
        foreach (ModifierParam p in System.Enum.GetValues(typeof(ModifierParam)))
        {
            var group = new List<EntryData>();
            foreach (var e in entries)
                if (e.Param == p) group.Add(e);
            if (group.Count == 0) continue;

            _paramFoldouts.TryGetValue(p, out bool open);

            // Вычисляем глобальный cap для этой группы
            int maxCap = 0;
            foreach (var e in group) if (e.MaxStacks > maxCap) maxCap = e.MaxStacks;
            bool capReached = maxCap > 0 && group.Count >= maxCap;

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

                    if (capReached)
                        GUILayout.Label("CAP", _capReachedStyle, GUILayout.Width(36));

                    DrawStackCapBadge(group.Count, maxCap);
                }

                _paramFoldouts[p] = open;
                if (!open) continue;

                EditorGUI.indentLevel++;
                EditorGUILayout.Space(2);

                // ── Шапка таблицы ────────────────────────────────────────
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("EffectId",  EditorStyles.miniBoldLabel, GUILayout.Width(160));
                    GUILayout.Label("SourceId",  EditorStyles.miniBoldLabel, GUILayout.Width(100));
                    GUILayout.Label("×",         EditorStyles.miniBoldLabel, GUILayout.Width(70));
                    GUILayout.Label("Δ",         EditorStyles.miniBoldLabel, GUILayout.Width(60));
                    GUILayout.Label("Counting",  EditorStyles.miniBoldLabel, GUILayout.Width(72));
                    GUILayout.Label("Cap",       EditorStyles.miniBoldLabel, GUILayout.Width(32));
                }

                DrawSeparator();

                string lastEffectId = null;
                for (int i = 0; i < group.Count; i++)
                {
                    var e = group[i];

                    if (lastEffectId != null && lastEffectId != e.Key.EffectId)
                        DrawSeparator();
                    lastEffectId = e.Key.EffectId;

                    var rowRect = GUILayoutUtility.GetRect(0, 18, GUILayout.ExpandWidth(true));
                    EditorGUI.DrawRect(rowRect, i % 2 == 0 ? ColRowEven : ColRowOdd);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label(TruncateId(e.Key.EffectId, 22),
                            EditorStyles.miniLabel, GUILayout.Width(160));
                        GUILayout.Label(TruncateId(e.Key.SourceId, 12),
                            EditorStyles.miniLabel, GUILayout.Width(100));

                        var prev = GUI.contentColor;

                        GUI.contentColor = MultiplierColor(e.Mult);
                        GUILayout.Label($"×{e.Mult:F4}", EditorStyles.miniLabel, GUILayout.Width(70));

                        float delta = e.Mult - 1f;
                        GUILayout.Label(delta >= 0f ? $"+{delta:P1}" : $"{delta:P1}",
                            EditorStyles.miniLabel, GUILayout.Width(60));

                        GUI.contentColor = prev;

                        // Режим подсчёта
                        GUI.contentColor = e.Counting == ModifierStackCounting.PerSource
                            ? new Color(0.6f, 0.8f, 1f)
                            : new Color(1f, 0.8f, 0.4f);
                        GUILayout.Label(e.Counting == ModifierStackCounting.PerSource
                            ? "PerSrc" : "OnTgt",
                            EditorStyles.miniLabel, GUILayout.Width(72));
                        GUI.contentColor = prev;

                        // Cap
                        GUI.contentColor = capReached ? ColCapReached : ColNeutral;
                        GUILayout.Label(e.MaxStacks <= 0 ? "∞" : e.MaxStacks.ToString(),
                            EditorStyles.miniLabel, GUILayout.Width(32));
                        GUI.contentColor = prev;
                    }
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(2);
        }
    }

    // ── Raw-таблица ──────────────────────────────────────────────────────────
    private void DrawRawTable(List<EntryData> entries)
    {
        _showRaw = EditorGUILayout.Foldout(_showRaw, $"Raw стаки ({entries.Count})", true);
        if (!_showRaw) return;

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("#",         EditorStyles.miniBoldLabel, GUILayout.Width(24));
                GUILayout.Label("Param",     EditorStyles.miniBoldLabel, GUILayout.Width(110));
                GUILayout.Label("EffectId",  EditorStyles.miniBoldLabel, GUILayout.Width(150));
                GUILayout.Label("SourceId",  EditorStyles.miniBoldLabel, GUILayout.Width(100));
                GUILayout.Label("×",         EditorStyles.miniBoldLabel, GUILayout.Width(70));
                GUILayout.Label("Counting",  EditorStyles.miniBoldLabel, GUILayout.Width(72));
                GUILayout.Label("Cap",       EditorStyles.miniBoldLabel, GUILayout.Width(32));
            }

            DrawSeparator();

            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];

                var rowRect = GUILayoutUtility.GetRect(0, 18, GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(rowRect, i % 2 == 0 ? ColRowEven : ColRowOdd);

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(i.ToString(),
                        EditorStyles.miniLabel, GUILayout.Width(24));
                    GUILayout.Label(e.Param.ToString(),
                        EditorStyles.miniLabel, GUILayout.Width(110));
                    GUILayout.Label(TruncateId(e.Key.EffectId, 20),
                        EditorStyles.miniLabel, GUILayout.Width(150));
                    GUILayout.Label(TruncateId(e.Key.SourceId, 12),
                        EditorStyles.miniLabel, GUILayout.Width(100));

                    var prev = GUI.contentColor;

                    GUI.contentColor = MultiplierColor(e.Mult);
                    GUILayout.Label($"×{e.Mult:F4}",
                        EditorStyles.miniLabel, GUILayout.Width(70));
                    GUI.contentColor = prev;

                    GUI.contentColor = e.Counting == ModifierStackCounting.PerSource
                        ? new Color(0.6f, 0.8f, 1f)
                        : new Color(1f, 0.8f, 0.4f);
                    GUILayout.Label(e.Counting == ModifierStackCounting.PerSource
                        ? "PerSrc" : "OnTgt",
                        EditorStyles.miniLabel, GUILayout.Width(72));
                    GUI.contentColor = prev;

                    GUI.contentColor = ColNeutral;
                    GUILayout.Label(e.MaxStacks <= 0 ? "∞" : e.MaxStacks.ToString(),
                        EditorStyles.miniLabel, GUILayout.Width(32));
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
            ? $"×{product:F3}  ▲ +{(product - 1f):P1}"
            : $"×{product:F3}  ▼ {(product - 1f):P1}";
        GUILayout.Label(label, _multiplierStyle, GUILayout.Width(170));
        GUI.contentColor = prev;
    }

    /// <summary>
    /// Бейдж "current / max". Оранжевый если cap достигнут, иначе обычный.
    /// </summary>
    private void DrawStackCapBadge(int current, int max)
    {
        bool capped = max > 0 && current >= max;
        string label = max <= 0 ? $"{current} / ∞" : $"{current} / {max}";
        var style = capped ? _capReachedStyle : _neutralBadge;
        GUILayout.Label(label, style, GUILayout.Width(52));
    }

    private static Color MultiplierColor(float v) =>
        Mathf.Approximately(v, 1f) ? ColNeutral : v > 1f ? ColBoost : ColPenalty;

    private static void DrawSeparator()
    {
        var rect = GUILayoutUtility.GetRect(0, 1, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(rect, ColSeparator);
        GUILayout.Space(1);
    }

    private static string TruncateId(string id, int max) =>
        string.IsNullOrEmpty(id) ? "—"
        : id.Length <= max       ? id
                                 : id.Substring(0, max - 1) + "…";

    private static List<EntryData> ReadEntries(System.Collections.IList rawList)
    {
        var result = new List<EntryData>(rawList.Count);
        foreach (var raw in rawList)
        {
            result.Add(new EntryData(
                (ModifierParam)_seParam.GetValue(raw),
                (EffectKey)_seKey.GetValue(raw),
                (float)_seMultiplier.GetValue(raw),
                (ModifierStackCounting)_seCounting.GetValue(raw),
                (int)_seMaxStacks.GetValue(raw)
            ));
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

        _capReachedStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal    = { textColor = ColCapReached }
        };

        _stylesReady = true;
    }
}
#endif