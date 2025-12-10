using System.Collections.Generic;
using System.IO;
using Arcatech.Texts;
using UnityEditor;
using UnityEngine;

public class DescriptionManagerWindow : EditorWindow
{
    private Vector2 _scrollPos;
    private string _newTitle = "New Description";
    private string _targetFolder = "Assets/Descriptions";
    private GUIStyle _cardStyle;
    private GUIStyle _cardTitleStyle;
    private GUIStyle _labelStyle;
    private const int Columns = 6;
    private const float CardWidth = 250f;

    [MenuItem("Window/Game/Description Manager")]
    public static void ShowWindow()
    {
        GetWindow<DescriptionManagerWindow>("Description Manager");
    }
    

    private void OnGUI()
    {
        _cardStyle = new GUIStyle("box")
        {
            padding = new RectOffset(12, 12, 12, 12),
            margin = new RectOffset(4, 4, 4, 4)
        };

        _cardTitleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleLeft
        };

        _labelStyle = new GUIStyle(EditorStyles.label)
        {
            fontStyle = FontStyle.Bold
        };
        
        EditorGUILayout.Space();
        DrawCreateSection();
        EditorGUILayout.Space();
        DrawDescriptionGroups();
    }

    private void DrawCreateSection()
    {
        EditorGUILayout.LabelField("Create New Description", EditorStyles.boldLabel);
        _newTitle = EditorGUILayout.TextField("Title", _newTitle);

        using (new EditorGUILayout.HorizontalScope())
        {
            _targetFolder = EditorGUILayout.TextField("Folder", _targetFolder);
            if (GUILayout.Button("Browse", GUILayout.Width(80)))
            {
                var selection = EditorUtility.OpenFolderPanel("Choose Description Folder", Application.dataPath, "");
                if (!string.IsNullOrEmpty(selection) && selection.StartsWith(Application.dataPath))
                {
                    _targetFolder = "Assets" + selection.Substring(Application.dataPath.Length);
                }
            }
        }

        if (GUILayout.Button("Create Description"))
        {
            CreateNewDescription(_newTitle, _targetFolder);
        }
    }

    private void DrawDescriptionGroups()
    {
        EditorGUILayout.LabelField("All Descriptions", EditorStyles.boldLabel);

        var grouped = GetDescriptionsGroupedByFolder();

        using (var scroll = new EditorGUILayout.ScrollViewScope(_scrollPos))
        {
            _scrollPos = scroll.scrollPosition;

            foreach (var kvp in grouped)
            {
                EditorGUILayout.LabelField(kvp.Key, EditorStyles.largeLabel);
                EditorGUILayout.Space();

                DrawGridForFolder(kvp.Value);
                EditorGUILayout.Space();
            }
        }
    }

    private void DrawGridForFolder(List<Description> descriptions)
    {
        if (descriptions.Count == 0) return;

        int count = 0;
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();

        foreach (var description in descriptions)
        {
            DrawDescriptionCard(description);
            count++;

            if (count % Columns == 0)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
            }
        }

        // fill remaining slots to keep alignment
        if (count % Columns != 0)
        {
            int empties = Columns - (count % Columns);
            for (int i = 0; i < empties; i++)
            {
                GUILayout.Space(CardWidth + 8);
            }
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private void DrawDescriptionCard(Description description)
    {
        using (new EditorGUILayout.VerticalScope(_cardStyle, GUILayout.Width(CardWidth)))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(description.Title ?? "<Untitled>", _cardTitleStyle);
                if (GUILayout.Button("Select", GUILayout.Width(70)))
                {
                    Selection.activeObject = description;
                    EditorGUIUtility.PingObject(description);
                }
            }

            if (description.Picture != null)
            {
                var preview = AssetPreview.GetAssetPreview(description.Picture);
                if (preview != null)
                {
                    GUILayout.Label(preview, GUILayout.Height(110), GUILayout.ExpandWidth(true));
                }
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Description", _labelStyle);
            EditorGUILayout.LabelField(description.Text ?? string.Empty, EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Flavor Text", _labelStyle);
            EditorGUILayout.LabelField(description.FlavorText ?? string.Empty, EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Duplicate"))
                {
                    DuplicateDescription(description);
                }

                if (GUILayout.Button("Delete"))
                {
                    DeleteDescription(description);
                }

                if (GUILayout.Button("Rename"))
                {
                    RenameDescription(description);
                }
            }
        }
    }

    private Dictionary<string, List<Description>> GetDescriptionsGroupedByFolder()
    {
        var grouped = new SortedDictionary<string, List<Description>>();
        var guids = AssetDatabase.FindAssets("t:Description");

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var folder = Path.GetDirectoryName(path) ?? "Assets";
            if (!grouped.ContainsKey(folder))
            {
                grouped[folder] = new List<Description>();
            }

            var description = AssetDatabase.LoadAssetAtPath<Description>(path);
            if (description != null)
            {
                grouped[folder].Add(description);
            }
        }

        return new Dictionary<string, List<Description>>(grouped);
    }

    private void CreateNewDescription(string title, string folder)
    {
        if (string.IsNullOrWhiteSpace(folder))
        {
            folder = "Assets";
        }

        if (!AssetDatabase.IsValidFolder(folder))
        {
            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
        }

        var instance = CreateInstance<Description>();
        instance.Title = title;

        var safeTitle = string.IsNullOrWhiteSpace(title) ? "Description" : title;
        var path = $"{folder}/{safeTitle}.asset";
        path = AssetDatabase.GenerateUniqueAssetPath(path);

        AssetDatabase.CreateAsset(instance, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = instance;
    }

    private void DuplicateDescription(Description original)
    {
        var path = AssetDatabase.GetAssetPath(original);
        var folder = Path.GetDirectoryName(path) ?? "Assets";
        var destPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folder, original.name + " Copy.asset"));

        AssetDatabase.CopyAsset(path, destPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        var copy = AssetDatabase.LoadAssetAtPath<Description>(destPath);
        Selection.activeObject = copy;
    }

    private void DeleteDescription(Description target)
    {
        var path = AssetDatabase.GetAssetPath(target);
        if (EditorUtility.DisplayDialog("Delete Description",
                $"Are you sure you want to delete \"{target.Title}\"?", "Delete", "Cancel"))
        {
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    private void RenameDescription(Description target)
    {
        var path = AssetDatabase.GetAssetPath(target);
        var directory = Path.GetDirectoryName(path) ?? "Assets";
        var newName = EditorUtility.SaveFilePanelInProject("Rename Description", target.name, "asset", "Enter a new name", directory);

        if (string.IsNullOrEmpty(newName))
        {
            return;
        }

        AssetDatabase.RenameAsset(path, Path.GetFileNameWithoutExtension(newName));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}