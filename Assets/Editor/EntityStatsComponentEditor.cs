// Editor/EntityStatsComponentEditor.cs
// Custom inspector for EntityStatsComponent with live visualization.

using System.Globalization;
using Arcatech.Stats;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EntityStatsComponent))]
public class EntityStatsComponentEditor : Editor
{
    private bool _showStats = true;
    private bool _showEffects = true;
    private bool _showPeriodic = false;
    private bool _showEquipment = false;

    private GUIStyle _headerStyle;
    private GUIStyle _miniBold;

    private void InitStyles()
    {
        if (_headerStyle == null)
        {
            _headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 12 };
        }
        if (_miniBold == null)
        {
            _miniBold = new GUIStyle(EditorStyles.miniBoldLabel);
        }
    }

    public override void OnInspectorGUI()
    {
        InitStyles();

        var comp = (EntityStatsComponent)target;

        // Draw serialized fields (startingConfig, preserveCurrentRatioOnMaxChange, events won't show here)
        EditorGUILayout.LabelField("Config", _headerStyle);
        DrawDefaultInspector();

        EditorGUILayout.Space();

        // Preview controls in Edit Mode
        if (!Application.isPlaying)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Preview From Config (Edit Mode)"))
                {
                    Undo.RecordObject(comp, "Preview Stats Init");
                    comp.InitializeFromConfig();
                    EditorUtility.SetDirty(comp);
                }

                using (new EditorGUI.DisabledScope(!comp.IsRuntimeInitializedForPreview()))
                {
                    if (GUILayout.Button("Clear Preview"))
                    {
                        // Re-initialize to empty by clearing startingConfig and re-initializing, or simply reset
                        Undo.RecordObject(comp, "Clear Preview");
                        comp.InitializeFromConfig(); // Will clear and re-read config; if config null => empty
                        EditorUtility.SetDirty(comp);
                    }
                }
            }
        }

        EditorGUILayout.Space();

        // Stats overview
        _showStats = EditorGUILayout.Foldout(_showStats, "Stats Overview", true);
        if (_showStats)
        {
            var stats = comp.GetDebugStats();
            if (stats.Length == 0)
            {
                EditorGUILayout.HelpBox("No stats present. Assign a UnitStatsConfig and Initialize/Play to populate.", MessageType.Info);
            }
            else
            {
                foreach (var s in stats)
                {
                    DrawStatBar(s);
                    DrawStatAggregation(s);
                    EditorGUILayout.Space(4);
                }
            }
        }

        EditorGUILayout.Space();

        // Active effects
        _showEffects = EditorGUILayout.Foldout(_showEffects, "Active Effects", true);
        if (_showEffects)
        {
            var effects = comp.GetDebugEffects();
            if (effects.Length == 0)
            {
                EditorGUILayout.LabelField("No active effects.");
            }
            else
            {
                foreach (var e in effects)
                {
                    using (new EditorGUILayout.VerticalScope("box"))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField(e.displayName, _miniBold);
                            if (e.effectAsset != null)
                            {
                                EditorGUILayout.ObjectField(e.effectAsset, typeof(StatsEffect), false, GUILayout.MaxWidth(200));
                            }
                        }

                        EditorGUILayout.LabelField($"Stacks: {e.stacks}");
                        EditorGUILayout.LabelField($"Remaining: {(e.secondsRemaining.HasValue ? e.secondsRemaining.Value.ToString("0.00s", CultureInfo.InvariantCulture) : "∞")}");

                        if (e.sourceRef != null)
                        {
                            EditorGUILayout.ObjectField("Source", e.sourceRef, typeof(Object), true);
                        }

                        if (e.persistentMaxMods != null && e.persistentMaxMods.Count > 0)
                        {
                            EditorGUILayout.LabelField("Persistent Max Modifiers:");
                            foreach (var m in e.persistentMaxMods)
                            {
                                EditorGUILayout.LabelField($"- {m.stat} • {m.target} • {(m.op == StatOpKind.Add ? "Add" : "Mult")} • {FormatModifierValue(m)}");
                            }
                        }
                    }
                }
            }
        }

        EditorGUILayout.Space();

        // Periodic processes
        _showPeriodic = EditorGUILayout.Foldout(_showPeriodic, "Periodic Deltas (Ticks)", true);
        if (_showPeriodic)
        {
            var pList = comp.GetDebugPeriodic();
            if (pList.Length == 0)
            {
                EditorGUILayout.LabelField("No periodic processes.");
            }
            else
            {
                foreach (var p in pList)
                {
                    using (new EditorGUILayout.VerticalScope("box"))
                    {
                        EditorGUILayout.LabelField($"{(p.isEquipment ? "Equipment" : "Effect")} • {p.sourceLabel}", _miniBold);
                        var d = p.spec.delta;

                        string sign = d.amount >= 0f ? "+" : "";
                        EditorGUILayout.LabelField($"Delta: {sign}{d.amount} → {d.stat} ({d.target})");
                        EditorGUILayout.LabelField($"Interval: {p.spec.intervalSeconds:0.###}s • Accumulator: {p.accumulator:0.###} • Stacks: {p.stacks}");
                        EditorGUILayout.LabelField($"Remaining: {(p.secondsRemaining.HasValue ? p.secondsRemaining.Value.ToString("0.00s", CultureInfo.InvariantCulture) : "∞")}");
                    }
                }
            }
        }

        EditorGUILayout.Space();

        // Equipment max modifiers
        _showEquipment = EditorGUILayout.Foldout(_showEquipment, "Equipment Max Modifiers", true);
        if (_showEquipment)
        {
            var equips = comp.GetDebugEquipmentModifiers();
            if (equips.Length == 0)
            {
                EditorGUILayout.LabelField("No equipment max modifiers.");
            }
            else
            {
                foreach (var eq in equips)
                {
                    using (new EditorGUILayout.VerticalScope("box"))
                    {
                        EditorGUILayout.LabelField(eq.sourceLabel, _miniBold);
                        foreach (var m in eq.maxModifiers)
                        {
                            EditorGUILayout.LabelField($"- {m.stat} • {m.target} • {(m.op == StatOpKind.Add ? "Add" : "Mult")} • {FormatModifierValue(m)}");
                        }
                    }
                }
            }
        }

        // Auto repaint in Play Mode for live updates
        if (Application.isPlaying)
        {
            Repaint();
        }
    }

    private void DrawStatBar(EntityStatsComponent.DebugStatView s)
    {
        EditorGUILayout.LabelField(s.stat.ToString(), _miniBold);

        float clampMax = s.maxClamp > 0f ? Mathf.Min(s.maxClamp, s.max) : s.max;
        string label = $"{s.current:0.##}/{s.max:0.##} ({(s.Ratio * 100f):0.#}%)";
        Rect r = GUILayoutUtility.GetRect(18, 18);
        EditorGUI.ProgressBar(r, Mathf.Clamp01(s.Ratio), label);

        // Clamp info
        if (s.minClamp != 0f || s.maxClamp != 0f)
        {
            EditorGUILayout.LabelField($"Clamp: [{s.minClamp:0.##}, {clampMax:0.##}]");
        }
    }

    private void DrawStatAggregation(EntityStatsComponent.DebugStatView s)
    {
        // Display the aggregation used: M = (B + A_equip + A_eff) * (1+m_equip) * (1+m_eff)
        EditorGUILayout.LabelField("Aggregation");
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label($"B={s.baseMax:0.##}", GUILayout.MinWidth(60));
            GUILayout.Label($"+Aeq={s.equipAddMax:0.##}", GUILayout.MinWidth(80));
            GUILayout.Label($"+Aef={s.effectAddMax:0.##}", GUILayout.MinWidth(80));
            GUILayout.Label($"×(1+meq)={(1f + s.equipMultMax):0.###}", GUILayout.MinWidth(120));
            GUILayout.Label($"×(1+mef)={(1f + s.effectMultMax):0.###}", GUILayout.MinWidth(120));
            GUILayout.FlexibleSpace();
        }
    }

    private static string FormatModifierValue(StatModifier m)
    {
        if (m.op == StatOpKind.Add) return m.value.ToString("0.###", CultureInfo.InvariantCulture);
        // Mult is stored as +m (use 0.10 for +10%)
        float pct = m.value * 100f;
        return $"+{pct:0.##}%";
    }
}