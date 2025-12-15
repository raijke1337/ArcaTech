// Assets/Editor/StatsEffectBrowserWindow.cs
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Arcatech.Stats;

public class StatsEffectBrowserWindow : EditorWindow
{
    private class FolderGroup
    {
        public string folderPath;
        public List<UsableEffect> items = new List<UsableEffect>();
        public bool expanded = true;
    }

    private Vector2 _scroll;
    private List<FolderGroup> _groups = new List<FolderGroup>();
    private string[] _folderOptions = new string[0];
    private int _createFolderIndex = 0;

    private string _search = "";
    private float _cardWidth = 260f;
    private float _thumbnailSize = 72f;

    // How many detail rows to show per section on each card
    private int _maxRowsPerSection = 4;

    private double _lastRefreshTime = 0;
    private const double AutoRefreshInterval = 1.0;

    [MenuItem("Window/Game/Effects/Stats Effect Browser")]
    public static void Open()
    {
        var win = GetWindow<StatsEffectBrowserWindow>("Stats Effects");
        win.minSize = new Vector2(500, 300);
        win.Show();
    }

    private void OnEnable()
    {
        RefreshData();
        EditorApplication.projectChanged += OnProjectChanged;
        wantsMouseMove = true;
    }

    private void OnDisable()
    {
        EditorApplication.projectChanged -= OnProjectChanged;
    }

    private void OnFocus()
    {
        RefreshData();
    }

    private void OnProjectChanged()
    {
        RefreshData();
        Repaint();
    }

    private void Update()
    {
        if (EditorApplication.timeSinceStartup - _lastRefreshTime > AutoRefreshInterval)
        {
            _lastRefreshTime = EditorApplication.timeSinceStartup;
        }
    }

    private void RefreshData()
    {
        var guids = AssetDatabase.FindAssets("t:Arcatech.Stats.StatsEffect t:UsableEffect");
        var all = new List<(string path, UsableEffect obj)>(guids.Length);

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var obj = AssetDatabase.LoadAssetAtPath<UsableEffect>(path);
            if (obj != null)
                all.Add((path, obj));
        }

        var grouped = all
            .GroupBy(x => NormalizeFolder(Path.GetDirectoryName(x.path) ?? "Assets"))
            .OrderBy(g => g.Key)
            .ToList();

        _groups.Clear();
        foreach (var g in grouped)
        {
            var fg = new FolderGroup
            {
                folderPath = g.Key,
                items = g.Select(x => x.obj)
                         .OrderBy(o => GetTitle(o))
                         .ThenBy(o => o.name)
                         .ToList(),
                expanded = true
            };
            _groups.Add(fg);
        }

        _folderOptions = _groups.Select(g => g.folderPath).ToArray();
        if (_createFolderIndex >= _folderOptions.Length) _createFolderIndex = 0;

        Repaint();
    }

    private static string NormalizeFolder(string folder)
    {
        if (string.IsNullOrEmpty(folder)) return "Assets";
        folder = folder.Replace("\\", "/");
        if (!folder.StartsWith("Assets")) return "Assets";
        return folder;
    }

    private void OnGUI()
    {
        DrawToolbar();

        if (_groups.Count == 0)
        {
            EditorGUILayout.HelpBox("No UsableEffect assets found. Click Create to make your first one.", MessageType.Info);
            GUILayout.FlexibleSpace();
            return;
        }

        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        float viewWidth = position.width - 32f;
        int columns = Mathf.Max(1, Mathf.FloorToInt((viewWidth) / (_cardWidth + 14f)));

        foreach (var group in _groups)
        {
            DrawGroupHeader(group);

            if (!group.expanded) continue;

            var list = string.IsNullOrEmpty(_search)
                ? group.items
                : group.items.Where(MatchesSearch).ToList();

            if (list.Count == 0)
            {
                EditorGUILayout.HelpBox("No items match the filter/search in this folder.", MessageType.None);
                continue;
            }

            int colCount = 0;
            EditorGUILayout.BeginHorizontal();
            foreach (var effect in list)
            {
                DrawEffectCard(effect);

                colCount++;
                if (colCount >= columns)
                {
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(6f);
                    EditorGUILayout.BeginHorizontal();
                    colCount = 0;
                }
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(8f);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawToolbar()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            // Search field
            using (new GUILayout.HorizontalScope(GUILayout.MaxWidth(380)))
            {
                GUILayout.Label(EditorGUIUtility.IconContent("Search Icon"), EditorStyles.toolbarButton, GUILayout.Width(24));
                var newSearch = GUILayout.TextField(_search, EditorStyles.toolbarTextField, GUILayout.ExpandWidth(true));
                if (newSearch != _search) _search = newSearch;

                if (GUILayout.Button(EditorGUIUtility.IconContent("winbtn_mac_close_h"), EditorStyles.toolbarButton, GUILayout.Width(24)))
                {
                    _search = "";
                    GUI.FocusControl(null);
                }
            }

            GUILayout.Space(8);

            // Card width slider
            GUILayout.Label("Card Width", EditorStyles.toolbarButton);
            _cardWidth = Mathf.Round(GUILayout.HorizontalSlider(_cardWidth, 220f, 460f, GUILayout.Width(140)));
            GUILayout.Space(8);

            // Detail rows slider
            GUILayout.Label("Detail Rows", EditorStyles.toolbarButton);
            _maxRowsPerSection = Mathf.RoundToInt(GUILayout.HorizontalSlider(_maxRowsPerSection, 1, 8, GUILayout.Width(90)));
            _maxRowsPerSection = Mathf.Clamp(_maxRowsPerSection, 1, 12);

            GUILayout.FlexibleSpace();

            // Create target folder popup
            GUILayout.Label("Create In", EditorStyles.toolbarButton);
            int newIndex = EditorGUILayout.Popup(_createFolderIndex, _folderOptions, EditorStyles.toolbarPopup, GUILayout.Width(250));
            if (newIndex != _createFolderIndex)
                _createFolderIndex = newIndex;

            if (GUILayout.Button(new GUIContent("Create New", "Create a new StatsEffect in the selected folder"), EditorStyles.toolbarButton))
            {
                CreateNewInSelectedFolder();
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent("Refresh"), EditorStyles.toolbarButton, GUILayout.Width(28)))
            {
                RefreshData();
            }
        }
    }

    private void DrawGroupHeader(FolderGroup group)
    {
        var headerRect = EditorGUILayout.GetControlRect(false, 22f);
        EditorGUI.DrawRect(headerRect, EditorGUIUtility.isProSkin
            ? new Color(0.18f, 0.18f, 0.18f)
            : new Color(0.85f, 0.85f, 0.85f));

        var foldRect = new Rect(headerRect.x + 6, headerRect.y + 3, 20, headerRect.height);
        group.expanded = EditorGUI.Foldout(foldRect, group.expanded, GUIContent.none, true);

        var label = $"{group.folderPath}  ({group.items.Count})";
        var labelRect = new Rect(foldRect.xMax + 2, headerRect.y + 2, headerRect.width - 140, headerRect.height);
        EditorGUI.LabelField(labelRect, label, EditorStyles.boldLabel);

        var createHereRect = new Rect(headerRect.xMax - 160, headerRect.y + 2, 120, headerRect.height - 4);
        if (GUI.Button(createHereRect, "Create Here"))
        {
            int idx = System.Array.IndexOf(_folderOptions, group.folderPath);
            if (idx >= 0) _createFolderIndex = idx;
            CreateNewInSelectedFolder();
        }

        var pingRect = new Rect(headerRect.xMax - 34, headerRect.y + 2, 30, headerRect.height - 4);
        if (GUI.Button(pingRect, EditorGUIUtility.IconContent("d_Folder Icon"), GUIStyle.none))
        {
            var obj = AssetDatabase.LoadAssetAtPath<Object>(group.folderPath);
            if (obj != null)
            {
                EditorGUIUtility.PingObject(obj);
                Selection.activeObject = obj;
            }
        }

        GUILayout.Space(2);
    }

    private void DrawEffectCard(UsableEffect effect)
    {
        using (new GUILayout.VerticalScope("box", GUILayout.Width(_cardWidth)))
        {
            // Header: thumbnail + title + asset name
            using (new GUILayout.HorizontalScope())
            {
                var thumb = GetPreviewTexture(effect);
                GUILayout.Label(thumb, GUILayout.Width(_thumbnailSize), GUILayout.Height(_thumbnailSize));

                using (new GUILayout.VerticalScope())
                {
                    // Title (Description.Title) and asset name
                    string title = GetTitle(effect);
                    GUILayout.Label(string.IsNullOrEmpty(title) ? "(No Title)" : title, EditorStyles.boldLabel, GUILayout.MaxHeight(20));

                    // Show the actual asset name distinctly
                    using (new GUILayout.HorizontalScope())
                    {
                        GUIStyle nameStyle = new GUIStyle(EditorStyles.miniLabel);
                        nameStyle.fontStyle = FontStyle.Italic;
                        GUILayout.Label($"Asset: {effect.name}", nameStyle);
                    }

                    string lifetime = effect.infiniteDuration ? "Lifetime: Infinite" : $"Lifetime: {effect.durationSeconds:0.##}s";
                    GUILayout.Label(lifetime, EditorStyles.miniLabel);

                    string stacking = effect.canStack ? $"Stacking: Up to {effect.maxStacks}" : "Stacking: No";
                    GUILayout.Label(stacking, EditorStyles.miniLabel);
                }
            }

            GUILayout.Space(4);

            // Summary chips (counts)
            using (new GUILayout.HorizontalScope())
            {
                DrawChip(new GUIContent($"Instant: {SafeCount(effect.instantDeltas)}", "Count of Instant Deltas"));
                DrawChip(new GUIContent($"Mods: {SafeCount(effect.persistentModifiers)}", "Count of Persistent Modifiers"));
                DrawChip(new GUIContent($"Periodic: {SafeCount(effect.periodicDeltas)}", "Count of Periodic Deltas"));
                GUILayout.FlexibleSpace();
            }

            GUILayout.Space(4);

            // Numeric details (compact)
            DrawSectionLines("Instant", BuildInstantLines(effect), _maxRowsPerSection);
            DrawSectionLines("Modifiers", BuildModifierLines(effect), _maxRowsPerSection);
            DrawSectionLines("Periodic", BuildPeriodicLines(effect), _maxRowsPerSection);

            GUILayout.Space(6);

            // Action buttons
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Open", GUILayout.Height(22)))
                {
                    Selection.activeObject = effect;
                    EditorGUIUtility.PingObject(effect);
                    EditorUtility.FocusProjectWindow();
                }
                if (GUILayout.Button("Duplicate", GUILayout.Height(22)))
                {
                    DuplicateAsset(effect);
                }
                var delStyle = new GUIStyle(GUI.skin.button);
                delStyle.normal.textColor = Color.red;
                if (GUILayout.Button("Delete", delStyle, GUILayout.Height(22)))
                {
                    DeleteAsset(effect);
                }
            }
        }
    }

    private void DrawChip(GUIContent content)
    {
        var style = new GUIStyle(EditorStyles.miniButtonMid);
        style.fontSize = 10;
        style.padding = new RectOffset(6, 6, 2, 2);
        style.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label(content, style, GUILayout.Height(18));
    }

    private void DrawSectionLines(string label, List<string> lines, int maxRows)
    {
        if (lines == null || lines.Count == 0) return;

        using (new GUILayout.VerticalScope())
        {
            var headerStyle = new GUIStyle(EditorStyles.miniBoldLabel);
            headerStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.9f) : new Color(0.15f, 0.15f, 0.2f);
            GUILayout.Label(label, headerStyle);

            int toShow = Mathf.Min(maxRows, lines.Count);
            for (int i = 0; i < toShow; i++)
            {
                GUILayout.Label("• " + lines[i], EditorStyles.miniLabel);
            }
            if (lines.Count > toShow)
            {
                int more = lines.Count - toShow;
                GUIStyle moreStyle = new GUIStyle(EditorStyles.miniLabel);
                moreStyle.fontStyle = FontStyle.Italic;
                moreStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.7f, 0.7f, 0.7f) : new Color(0.35f, 0.35f, 0.35f);
                GUILayout.Label($"+ {more} more…", moreStyle);
            }
        }
        GUILayout.Space(2);
    }

    private string GetTitle(UsableEffect effect)
    {
        try
        {
            return effect != null && effect.description != null ? effect.description.Title : effect?.name;
        }
        catch
        {
            return effect?.name;
        }
    }

    private Texture GetPreviewTexture(UsableEffect effect)
    {
        try
        {
            var desc = effect != null ? effect.description : null;
            var pic = desc != null ? desc.Picture : null;
            if (pic != null)
            {
                var prev = AssetPreview.GetAssetPreview(pic);
                if (prev != null) return prev;
                return pic.texture;
            }

            var fallback = AssetPreview.GetMiniThumbnail(effect);
            if (fallback != null) return fallback;

            return EditorGUIUtility.IconContent("ScriptableObject Icon").image;
        }
        catch
        {
            return EditorGUIUtility.IconContent("console.warnicon").image;
        }
    }

    private bool MatchesSearch(UsableEffect effect)
    {
        if (string.IsNullOrWhiteSpace(_search)) return true;

        string s = _search.ToLowerInvariant();
        if (effect == null) return false;

        if (!string.IsNullOrEmpty(effect.name) && effect.name.ToLowerInvariant().Contains(s))
            return true;

        try
        {
            var desc = effect.description;
            if (desc != null)
            {
                if (!string.IsNullOrEmpty(desc.Title) && desc.Title.ToLowerInvariant().Contains(s))
                    return true;
                if (!string.IsNullOrEmpty(desc.Text) && desc.Text.ToLowerInvariant().Contains(s))
                    return true;
                if (!string.IsNullOrEmpty(desc.FlavorText) && desc.FlavorText.ToLowerInvariant().Contains(s))
                    return true;
            }
        }
        catch { /* ignore */ }

        return false;
    }

    private void CreateNewInSelectedFolder()
    {
        string folder = (_folderOptions != null && _folderOptions.Length > 0 && _createFolderIndex >= 0 && _createFolderIndex < _folderOptions.Length)
            ? _folderOptions[_createFolderIndex]
            : "Assets";

        if (!AssetDatabase.IsValidFolder(folder))
        {
            EditorUtility.DisplayDialog("Invalid Folder", $"Target folder not found:\n{folder}", "OK");
            return;
        }

        string baseName = "StatsEffect";
        string path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folder, $"{baseName}.asset"));

        var asset = ScriptableObject.CreateInstance<UsableEffect>();
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = asset;
        EditorGUIUtility.PingObject(asset);

        RefreshData();
    }

    private void DuplicateAsset(UsableEffect effect)
    {
        if (effect == null) return;
        string src = AssetDatabase.GetAssetPath(effect);
        if (string.IsNullOrEmpty(src)) return;

        string dst = AssetDatabase.GenerateUniqueAssetPath(src);
        if (AssetDatabase.CopyAsset(src, dst))
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            var newObj = AssetDatabase.LoadAssetAtPath<UsableEffect>(dst);
            Selection.activeObject = newObj != null ? newObj : effect;
            if (newObj != null) EditorGUIUtility.PingObject(newObj);
            RefreshData();
        }
        else
        {
            EditorUtility.DisplayDialog("Duplicate Failed", "Could not duplicate the asset.", "OK");
        }
    }

    private void DeleteAsset(UsableEffect effect)
    {
        if (effect == null) return;
        string path = AssetDatabase.GetAssetPath(effect);
        if (string.IsNullOrEmpty(path)) return;

        bool ok = EditorUtility.DisplayDialog(
            "Delete StatsEffect",
            $"Are you sure you want to delete:\n{effect.name}\n\nThis cannot be undone.",
            "Delete",
            "Cancel");

        if (!ok) return;

        AssetDatabase.DeleteAsset(path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        RefreshData();
    }

    private static int SafeCount<T>(ICollection<T> col) => col == null ? 0 : col.Count;

    // ---------- Formatting helpers for numeric summaries ----------

    private List<string> BuildInstantLines(UsableEffect effect)
    {
        var lines = new List<string>();
        if (effect?.instantDeltas == null) return lines;

        foreach (var d in effect.instantDeltas)
        {
            lines.Add($"{d.stat} {d.target}: {Signed(d.amount)}");
        }
        return lines;
    }

    private List<string> BuildModifierLines(UsableEffect effect)
    {
        var lines = new List<string>();
        if (effect?.persistentModifiers == null) return lines;

        foreach (var m in effect.persistentModifiers)
        {
            if (m.op == StatOpKind.Add)
            {
                lines.Add($"{m.stat} {m.target}: Add {Signed(m.value)}");
            }
            else // Mult
            {
                float pct = m.value * 100f;
                float mult = 1f + m.value;
                lines.Add($"{m.stat} {m.target}: Mult +{pct:0.#}% (x{mult:0.##})");
            }
        }
        return lines;
    }

    private List<string> BuildPeriodicLines(UsableEffect effect)
    {
        var lines = new List<string>();
        if (effect?.periodicDeltas == null) return lines;

        foreach (var p in effect.periodicDeltas)
        {
            var d = p.delta;
            lines.Add($"{d.stat} {d.target}: {Signed(d.amount)} every {p.intervalSeconds:0.##}s");
        }
        return lines;
    }

    private string Signed(float v)
    {
        if (Mathf.Approximately(v, 0f)) return "0";
        return v > 0 ? $"+{v:0.##}" : $"{v:0.##}";
    }
}