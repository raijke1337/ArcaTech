// File: Assets/Editor/StateGraphEditorWindow.cs
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Arcatech.Units;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Simple State Graph Editor window for managing SerializedUnitState and SerializedStateTransition assets.
/// Shows a list of states on the left and an inspector for the selected state on the right.
/// Provides helpers to create states/transitions and basic validation.
/// </summary>
public class StateGraphEditorWindow : EditorWindow
{
    Vector2 _leftScroll;
    Vector2 _rightScroll;

    List<SerializedUnitState> _allStates = new List<SerializedUnitState>();
    SerializedUnitState _selectedState;
    Editor _selectedStateEditor;

    string _statesFolder = "Assets/Resources/States";
    string _newStateName = "NewState";
    string _newTransitionName = "NewTransition";

    [MenuItem("Window/State Graph Editor")]
    public static void Open()
    {
        GetWindow<StateGraphEditorWindow>("State Graph Editor");
    }

    void OnEnable()
    {
        RefreshStateList();
        Selection.selectionChanged += OnSelectionChanged;
    }

    void OnDisable()
    {
        Selection.selectionChanged -= OnSelectionChanged;
    }

    void OnSelectionChanged()
    {
        // If user selects a SerializedUnitState in Project, reflect it in the window
        var so = Selection.activeObject as SerializedUnitState;
        if (so != null)
        {
            SelectState(so);
            Repaint();
        }
    }

    void RefreshStateList()
    {
        _allStates.Clear();
        var guids = AssetDatabase.FindAssets("t:SerializedUnitState");
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var so = AssetDatabase.LoadAssetAtPath<SerializedUnitState>(path);
            if (so != null) _allStates.Add(so);
        }

        // try to keep a valid selected
        if (_selectedState != null && !_allStates.Contains(_selectedState))
        {
            _selectedState = null;
            ClearSelectedEditor();
        }
    }

    void ClearSelectedEditor()
    {
        if (_selectedStateEditor != null)
        {
            DestroyImmediate(_selectedStateEditor);
            _selectedStateEditor = null;
        }
    }

    void SelectState(SerializedUnitState state)
    {
        _selectedState = state;
        ClearSelectedEditor();
        if (_selectedState != null)
            _selectedStateEditor = Editor.CreateEditor(_selectedState);
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();

        // Left pane - state list and creation
        DrawLeftPane();

        // Right pane - selected state inspector and helpers
        DrawRightPane();

        EditorGUILayout.EndHorizontal();
    }

    void DrawLeftPane()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(260));
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("States", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh", GUILayout.Width(80)))
            RefreshStateList();

        if (GUILayout.Button("Ping Folder", GUILayout.Width(80)))
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(_statesFolder));

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // New state creation
        EditorGUILayout.BeginHorizontal();
        _newStateName = EditorGUILayout.TextField(_newStateName);
        if (GUILayout.Button("New State", GUILayout.Width(90)))
        {
            CreateNewStateAsset(_newStateName);
            RefreshStateList();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // State list
        _leftScroll = EditorGUILayout.BeginScrollView(_leftScroll);
        foreach (var s in _allStates.OrderBy(x => x.stateDisplayName))
        {
            EditorGUILayout.BeginHorizontal();
            bool isSelected = s == _selectedState;
            GUIStyle buttonStyle = isSelected ? EditorStyles.whiteLabel : EditorStyles.label;

            if (GUILayout.Button(s.stateDisplayName ?? s.name, buttonStyle))
            {
                SelectState(s);
            }

            if (GUILayout.Button("Ping", GUILayout.Width(44)))
            {
                Selection.activeObject = s;
                EditorGUIUtility.PingObject(s);
            }

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();
    }

    void DrawRightPane()
    {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.Space();

        if (_selectedState == null)
        {
            EditorGUILayout.HelpBox("Select a state from the list, or create a new one.", MessageType.Info);
            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.LabelField($"Editing: {_selectedState.stateDisplayName}", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save"))
        {
            EditorUtility.SetDirty(_selectedState);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        if (GUILayout.Button("Validate"))
        {
            ValidateSelectedState();
        }
        if (GUILayout.Button("Ping Asset"))
        {
            Selection.activeObject = _selectedState;
            EditorGUIUtility.PingObject(_selectedState);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Inline inspector for the selected state
        _rightScroll = EditorGUILayout.BeginScrollView(_rightScroll);
        if (_selectedStateEditor != null)
        {
            _selectedStateEditor.OnInspectorGUI();
        }
        else
        {
            EditorGUILayout.HelpBox("Could not create inspector for selected state.", MessageType.Warning);
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        // Quick helpers: create transition and attach to selected state
        EditorGUILayout.BeginHorizontal();
        _newTransitionName = EditorGUILayout.TextField(_newTransitionName);
        if (GUILayout.Button("Create Transition", GUILayout.Width(140)))
        {
            CreateNewTransitionAndAttach(_newTransitionName, _selectedState);
            EditorUtility.SetDirty(_selectedState);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            RefreshStateList(); // in case we created states during transition wiring
        }
        if (GUILayout.Button("Auto-include referenced states", GUILayout.Width(200)))
        {
            AutoIncludeReferencedStates(_selectedState);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            RefreshStateList();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.EndVertical();
    }

    // Create a new SerializedUnitState asset in _statesFolder
    void CreateNewStateAsset(string name)
    {
        EnsureFolderExists(_statesFolder);
// Create a unique asset path in the folder
        string assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(_statesFolder, name + ".asset"));
        var inst = CreateInstance<SerializedUnitState>();
        inst.stateDisplayName = Path.GetFileNameWithoutExtension(assetPath); // nicer display name
        AssetDatabase.CreateAsset(inst, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        SelectState(inst);
    }

    // Create a new SerializedStateTransition asset and append it to state.transitions
    void CreateNewTransitionAndAttach(string transitionName, SerializedUnitState state)
    {
        if (state == null) return;
        EnsureFolderExists(_statesFolder);

        // Generate unique path for the transition asset
        string assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(_statesFolder, transitionName + ".asset"));

        var inst = CreateInstance<SerializedStateTransition>();
        inst.name = Path.GetFileNameWithoutExtension(assetPath);
        AssetDatabase.CreateAsset(inst, assetPath);
        // Append to state's transitions array
        var so = new SerializedObject(state);
        var prop = so.FindProperty("transitions");
        if (prop == null)
        {
            Debug.LogError("Could not find transitions property on selected state.");
            return;
        }
        int newIndex = prop.arraySize;
        prop.InsertArrayElementAtIndex(newIndex);
        prop.GetArrayElementAtIndex(newIndex).objectReferenceValue = inst;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(state);

        // Select the new transition asset in the project window for immediate edit
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = inst;
        EditorGUIUtility.PingObject(inst);
    }

    // Simple validation: check transitions that have null nextState, null conditions or null actions
    void ValidateSelectedState()
    {
        if (_selectedState == null) return;
        var problems = new List<string>();

        var trans = _selectedState.transitions ?? new SerializedStateTransition[0];
        for (int i = 0; i < trans.Length; i++)
        {
            var t = trans[i];
            if (t == null)
            {
                problems.Add($"Transition #{i} is NULL.");
                continue;
            }
            if (t.nextState == null)
                problems.Add($"Transition '{t.name}' (Priority {t.Priority}) has no nextState assigned.");
            if (t.conditions == null || t.conditions.Length == 0)
                problems.Add($"Transition '{t.name}' has no conditions (will always be true).");
            if (t.onTransition == null || t.onTransition.Length == 0)
                ; // OK: not all transitions need onTransition
        }

        if (problems.Count == 0)
        {
            EditorUtility.DisplayDialog("Validation", "No problems detected in selected state.", "OK");
        }
        else
        {
            var msg = string.Join("\n", problems);
            EditorUtility.DisplayDialog("Validation Problems", msg, "OK");
        }
    }

    // Ensure a folder exists at path
    void EnsureFolderExists(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder)) return;
        string parent = Path.GetDirectoryName(folder);
        string newFolder = Path.GetFileName(folder);
        if (!string.IsNullOrEmpty(parent))
            EnsureFolderExists(parent);
        AssetDatabase.CreateFolder(parent, newFolder);
    }

    // Recursively find all referenced states from this state's transitions and add them to the AssetDatabase folder if missing.
    // This helper will also open the referenced next states in the list for editing (adds discoverability).
    void AutoIncludeReferencedStates(SerializedUnitState state)
    {
        if (state == null) return;
        var needToRefresh = false;

        var trans = state.transitions ?? new SerializedStateTransition[0];
        foreach (var t in trans)
        {
            if (t == null) continue;
            if (t.nextState == null) continue;
            // If nextState is an asset in the project, it's already included; otherwise save it
            string path = AssetDatabase.GetAssetPath(t.nextState);
            if (string.IsNullOrEmpty(path))
            {
                // nextState is likely an instance in-memory (rare). Save it as asset.
                EnsureFolderExists(_statesFolder);
                string name = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(_statesFolder, t.nextState.stateDisplayName + ".asset"));
                AssetDatabase.CreateAsset(t.nextState, name);
                needToRefresh = true;
            }
        }

        if (needToRefresh) AssetDatabase.Refresh();
    }
}
#endif