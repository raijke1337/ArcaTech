#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Arcatech.Stats;
using AYellowpaper.SerializedCollections;
using UnityEditor;
using UnityEngine;

public class BaseStatsConfigBrowserWindow : EditorWindow
{
    private const string WindowTitle = "Base Stats Browser";

    private static readonly ResourceStatType[] AllResourceTypes =
    {
        ResourceStatType.Health,
        ResourceStatType.Stamina,
        ResourceStatType.Energy
    };

    private List<BaseStatsConfig> _configs;
    private Vector2 _scrollPosition;

    [MenuItem("Arcatech/" + WindowTitle)]
    public static void ShowWindow()
    {
        var window = GetWindow<BaseStatsConfigBrowserWindow>(WindowTitle);
        window.minSize = new Vector2(800, 400);
    }

    private void OnEnable()
    {
        RefreshAssets();
    }

    private void RefreshAssets()
    {
        _configs = AssetDatabase
            .FindAssets("t:BaseStatsConfig")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<BaseStatsConfig>)
            .Where(c => c != null)
            .ToList();
    }

    private void OnGUI()
    {
        DrawToolbar();

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        if (_configs == null || _configs.Count == 0)
        {
            EditorGUILayout.HelpBox("No BaseStatsConfig assets found in project.", MessageType.Info);
        }
        else
        {
            foreach (var config in _configs)
                DrawConfigCard(config);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        // Prominent refresh button with icon-style label
        GUI.backgroundColor = new Color(0.85f, 0.92f, 1f);
        if (GUILayout.Button("⟳ Refresh Assets", ToolbarButtonStyle, GUILayout.Width(140)))
            RefreshAssets();
        GUI.backgroundColor = Color.white;

        GUILayout.Label($"   Found: {_configs?.Count ?? 0} configs", EditorStyles.toolbarPopup);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawConfigCard(BaseStatsConfig config)
    {
        EditorGUILayout.Space(10);

        // ── Outer card (no fixedWidth → stretches to window width) ──
        EditorGUILayout.BeginVertical(ConfigCardStyle);

        // ── Header row: full asset name + clickable reference ──
        EditorGUILayout.BeginHorizontal();

        // Large, readable title with full asset name
        EditorGUILayout.LabelField(config.name, HeaderTextStyle, GUILayout.MinWidth(100));

        GUILayout.FlexibleSpace();

        // Compact ObjectField on the right for click-to-ping/select
        using (new LabelWidthScope(0))
        {
            EditorGUILayout.ObjectField(
                config,
                typeof(BaseStatsConfig),
                false,
                GUILayout.Width(120));
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(6);

        // ── Resource cards row ──
        EditorGUILayout.BeginHorizontal();

        foreach (var resourceType in AllResourceTypes)
        {
            bool hasResource = config.resources != null
                               && config.resources.ContainsKey(resourceType);

            if (hasResource)
                DrawResourceCard(config, resourceType);
            else
                DrawEmptyResourceCard(config, resourceType);
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private void DrawResourceCard(BaseStatsConfig config, ResourceStatType resourceType)
    {
        var resource = config.resources[resourceType];

        EditorGUILayout.BeginVertical(ResourceCardStyle);

        // Title + remove button
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(resourceType.ToString(), BoldCenteredLabel);

        GUI.backgroundColor = new Color(0.9f, 0.6f, 0.6f);
        if (GUILayout.Button("×", MiniButtonStyle, GUILayout.Width(20), GUILayout.Height(16)))
        {
            RemoveResource(config, resourceType);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(3);

        EditorGUI.BeginChangeCheck();

        using (new LabelWidthScope(78))
        {
            resource.baseMax         = EditorGUILayout.IntField("Base Max",      resource.baseMax);
            resource.startCurrent    = EditorGUILayout.IntField("Start Current", resource.startCurrent);
            resource.minClampCurrent = EditorGUILayout.IntField("Min Clamp",     resource.minClampCurrent);
            resource.maxClampCurrent = EditorGUILayout.IntField("Max Clamp",     resource.maxClampCurrent);

            EditorGUILayout.Space(3);

            resource.setStartCurrentAsPercentOfMax =
                EditorGUILayout.Toggle("Start %", resource.setStartCurrentAsPercentOfMax);

            using (new EditorGUI.DisabledScope(!resource.setStartCurrentAsPercentOfMax))
            {
                resource.startPercent = EditorGUILayout.Slider("Percent", resource.startPercent, 0f, 1f);
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(config, $"Edit {resourceType}");
            config.resources[resourceType] = resource;
            EditorUtility.SetDirty(config);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawEmptyResourceCard(BaseStatsConfig config, ResourceStatType resourceType)
    {
        EditorGUILayout.BeginVertical(ResourceCardStyle);

        EditorGUILayout.LabelField(resourceType.ToString(), BoldCenteredLabel);
        EditorGUILayout.Space(8);

        GUI.backgroundColor = new Color(0.6f, 0.85f, 0.6f);
        if (GUILayout.Button("Assign", GUILayout.Height(28)))
        {
            Undo.RecordObject(config, $"Assign {resourceType}");

            if (config.resources == null)
                config.resources = new SerializedDictionary<ResourceStatType, UnitResource>();

            config.resources[resourceType] = new UnitResource
            {
                baseMax                       = 100,
                startCurrent                  = 100,
                minClampCurrent               = 0,
                maxClampCurrent               = 100,
                setStartCurrentAsPercentOfMax = false,
                startPercent                  = 1f
            };

            EditorUtility.SetDirty(config);
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndVertical();
    }

    private void RemoveResource(BaseStatsConfig config, ResourceStatType resourceType)
    {
        Undo.RecordObject(config, $"Remove {resourceType}");
        config.resources.Remove(resourceType);
        EditorUtility.SetDirty(config);
    }

    // ─────────── Utility ───────────

    private readonly struct LabelWidthScope : System.IDisposable
    {
        private readonly float _previous;
        public LabelWidthScope(float width)
        {
            _previous = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = width;
        }
        public void Dispose() => EditorGUIUtility.labelWidth = _previous;
    }

    // ─────────── Cached styles ───────────

    private static GUIStyle _configCardStyle;
    private static GUIStyle ConfigCardStyle =>
        _configCardStyle ??= new GUIStyle("box")
        {
            padding = new RectOffset(12, 12, 10, 12),
            margin  = new RectOffset(5, 5, 5, 5)
        };

    private static GUIStyle _resourceCardStyle;
    private static GUIStyle ResourceCardStyle =>
        _resourceCardStyle ??= new GUIStyle("helpbox")
        {
            padding    = new RectOffset(8, 8, 8, 8),
            margin     = new RectOffset(3, 3, 0, 0),
            fixedWidth = 220
        };

    private static GUIStyle _headerTextStyle;
    private static GUIStyle HeaderTextStyle =>
        _headerTextStyle ??= new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize  = 14,
            wordWrap  = false,
            alignment = TextAnchor.MiddleLeft
        };

    private static GUIStyle _boldCenteredLabel;
    private static GUIStyle BoldCenteredLabel =>
        _boldCenteredLabel ??= new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter
        };

    private static GUIStyle _miniButtonStyle;
    private static GUIStyle MiniButtonStyle =>
        _miniButtonStyle ??= new GUIStyle(EditorStyles.miniButton)
        {
            fontSize  = 10,
            fontStyle = FontStyle.Bold
        };

    private static GUIStyle _toolbarButtonStyle;
    private static GUIStyle ToolbarButtonStyle =>
        _toolbarButtonStyle ??= new GUIStyle(EditorStyles.toolbarButton)
        {
            fontStyle = FontStyle.Bold,
            fontSize  = 12
        };
}
#endif