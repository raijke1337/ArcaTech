using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Arcatech.Units;

namespace StateMachineEditor
{
    public class StateMachineEditorWindow : EditorWindow
    {
        private List<SerializedUnitState> allStates = new List<SerializedUnitState>();
        private List<SerializedStateTransition> allTransitions = new List<SerializedStateTransition>();
        private SerializedUnitState selectedEntryState;
        private Vector2 statesScrollPos;
        private Vector2 detailsScrollPos;
        private string searchFilter = "";
        private bool showReachabilityAnalysis = false;
        private HashSet<SerializedUnitState> reachableStates = new HashSet<SerializedUnitState>();

        [MenuItem("Window/State Machine Editor")]
        public static void Open()
        {
            GetWindow<StateMachineEditorWindow>("State Machine Editor");
        }

        private void OnGUI()
        {
            DrawToolbar();
            
            if (allStates.Count == 0)
            {
                EditorGUILayout.HelpBox("No states found. Create SerializedUnitState assets or search in specific folders.", MessageType.Info);
                return;
            }

            DrawMainEditor();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Scan All", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                ScanAllStates();
            }

            if (GUILayout.Button("Scan Folder", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                ScanFolder();
            }

            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                allStates.Clear();
                allTransitions.Clear();
                selectedEntryState = null;
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Validate", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                ValidateGraph();
                showReachabilityAnalysis = true;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            searchFilter = EditorGUILayout.TextField(searchFilter, EditorStyles.toolbarSearchField);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField($"States: {allStates.Count} | Transitions: {allTransitions.Count}", EditorStyles.helpBox);
        }

        private void DrawMainEditor()
        {
            EditorGUILayout.BeginHorizontal();

            // Left panel - State list and entry point
            EditorGUILayout.BeginVertical(GUILayout.Width(300));
            DrawStatesList();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Separator();

            // Right panel - State details and transitions with scrolling
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            detailsScrollPos = EditorGUILayout.BeginScrollView(detailsScrollPos);
            DrawStateDetails();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawStatesList()
        {
            EditorGUILayout.LabelField("States", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Height(100));
            statesScrollPos = EditorGUILayout.BeginScrollView(statesScrollPos);

            var filteredStates = allStates.Where(s => s.stateDisplayName.ToLower().Contains(searchFilter.ToLower())).ToList();

            foreach (var state in filteredStates)
            {
                DrawStateListItem(state);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Entry Point", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            selectedEntryState = EditorGUILayout.ObjectField("", selectedEntryState, typeof(SerializedUnitState), false) as SerializedUnitState;
            if (EditorGUI.EndChangeCheck())
            {
                ValidateGraph();
            }

            EditorGUILayout.HelpBox("Select the initial state for reachability analysis.", MessageType.Info, true);
        }

        private void DrawStateListItem(SerializedUnitState state)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            // Color indicator for reachability
            Color stateColor = Color.white;
            if (showReachabilityAnalysis)
            {
                stateColor = reachableStates.Contains(state) ? Color.green : Color.red;
            }

            GUI.backgroundColor = stateColor;
            if (GUILayout.Button(state.stateDisplayName, EditorStyles.miniButton, GUILayout.ExpandWidth(true)))
            {
                EditorGUIUtility.PingObject(state);
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
        }

        private void DrawStateDetails()
        {
            if (selectedEntryState == null)
            {
                EditorGUILayout.HelpBox("Select a state to view details.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField($"State: {selectedEntryState.stateDisplayName}", EditorStyles.boldLabel);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("State Properties", EditorStyles.boldLabel);
            DrawStateProperties(selectedEntryState);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Outgoing Transitions", EditorStyles.boldLabel);
            DrawOutgoingTransitions(selectedEntryState);
        }

        private void DrawStateProperties(SerializedUnitState state)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("Identity", EditorStyles.miniLabel);
            state.stateDisplayName = EditorGUILayout.TextField("Display Name", state.stateDisplayName);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Animator", EditorStyles.miniLabel);
            state.animatorStateName = EditorGUILayout.TextField("Animator State", state.animatorStateName);
            state.animatorLayer = EditorGUILayout.IntField("Layer", state.animatorLayer);
            state.crossfadeTime = EditorGUILayout.FloatField("Crossfade Time", state.crossfadeTime);
            state.minTimeInStateNormalized = EditorGUILayout.Slider("Min Time (Normalized)", state.minTimeInStateNormalized, 0, 1);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Gameplay Locks", EditorStyles.miniLabel);
            state.allowsMovement = EditorGUILayout.Toggle("Allows Movement", state.allowsMovement);
            state.allowsAiming = EditorGUILayout.Toggle("Allows Aiming", state.allowsAiming);
            state.invulnerable = EditorGUILayout.Toggle("Invulnerable", state.invulnerable);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Root Motion", EditorStyles.miniLabel);
            state.rootMotionEnabled = EditorGUILayout.Toggle("Root Motion Enabled", state.rootMotionEnabled);

            EditorGUI.indentLevel--;

            EditorUtility.SetDirty(state);
        }

        private void DrawOutgoingTransitions(SerializedUnitState fromState)
        {
            var outgoingTransitions = allTransitions.Where(t => t.nextState == null || IsTransitionFrom(t, fromState)).ToList();

            if (outgoingTransitions.Count == 0)
            {
                EditorGUILayout.HelpBox("No outgoing transitions found.", MessageType.Info);
                return;
            }

            // Sort by priority
            outgoingTransitions = outgoingTransitions.OrderByDescending(t => t.Priority).ToList();

            foreach (var transition in outgoingTransitions)
            {
                DrawTransitionPreview(transition);
            }
        }

        private bool IsTransitionFrom(SerializedStateTransition transition, SerializedUnitState fromState)
        {
            // Check if this transition originates from the given state
            // This requires checking where the transition is referenced in the state
            return fromState.transitions.Contains(transition);
        }

        private void DrawTransitionPreview(SerializedStateTransition transition)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"→ {transition.nextState?.stateDisplayName ?? "Unknown"}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Priority: {transition.Priority}", EditorStyles.miniLabel, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField($"Min Time: {transition.minTimeInSourceStateNormalized:F2}", EditorStyles.miniLabel);

            if (transition.conditions.Length > 0)
            {
                EditorGUILayout.LabelField($"Conditions: {transition.conditions.Length}", EditorStyles.miniLabel);
                EditorGUI.indentLevel++;

                foreach (var condition in transition.conditions)
                {
                    DrawConditionSummary(condition);
                }

                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.LabelField("No conditions (always transitions)", EditorStyles.miniLabel);
            }

            EditorGUI.indentLevel--;

            if (GUILayout.Button("Edit", GUILayout.Height(20)))
            {
                EditorGUIUtility.PingObject(transition);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawConditionSummary(SerializedStateTransitionCondition condition)
        {
            if (condition == null) return;

            var fields = condition.GetType().GetFields(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            string summary = condition.ConditionName + " (";
            var fieldValues = new List<string>();

            foreach (var field in fields)
            {
                var value = field.GetValue(condition);
                fieldValues.Add($"{ObjectNames.NicifyVariableName(field.Name)}: {value}");
            }

            summary += string.Join(", ", fieldValues) + ")";
            EditorGUILayout.LabelField(summary, EditorStyles.miniLabel);
        }

        private void ScanAllStates()
        {
            allStates.Clear();
            allTransitions.Clear();

            // Find all SerializedUnitState assets
            string[] guids = AssetDatabase.FindAssets("t:SerializedUnitState");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var state = AssetDatabase.LoadAssetAtPath<SerializedUnitState>(path);
                if (state != null)
                {
                    allStates.Add(state);
                }
            }

            // Find all SerializedStateTransition assets
            guids = AssetDatabase.FindAssets("t:SerializedStateTransition");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var transition = AssetDatabase.LoadAssetAtPath<SerializedStateTransition>(path);
                if (transition != null)
                {
                    allTransitions.Add(transition);
                }
            }

            Debug.Log($"Found {allStates.Count} states and {allTransitions.Count} transitions");
        }

        private void ScanFolder()
        {
            string folderPath = EditorUtility.OpenFolderPanel("Select Folder to Scan", "Assets", "");
            if (string.IsNullOrEmpty(folderPath)) return;

            folderPath = folderPath.Replace(Application.dataPath, "Assets");

            allStates.Clear();
            allTransitions.Clear();

            // Find all SerializedUnitState assets in folder
            string[] guids = AssetDatabase.FindAssets("t:SerializedUnitState", new[] { folderPath });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var state = AssetDatabase.LoadAssetAtPath<SerializedUnitState>(path);
                if (state != null)
                {
                    allStates.Add(state);
                }
            }

            // Find all SerializedStateTransition assets in folder
            guids = AssetDatabase.FindAssets("t:SerializedStateTransition", new[] { folderPath });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var transition = AssetDatabase.LoadAssetAtPath<SerializedStateTransition>(path);
                if (transition != null)
                {
                    allTransitions.Add(transition);
                }
            }

            Debug.Log($"Scanned {folderPath}: Found {allStates.Count} states and {allTransitions.Count} transitions");
        }

        private void ValidateGraph()
        {
            if (selectedEntryState == null)
            {
                EditorUtility.DisplayDialog("Error", "Select an entry point state first.", "OK");
                return;
            }

            reachableStates = FindReachableStates(selectedEntryState);
            int unreachableCount = allStates.Count - reachableStates.Count;

            string message = $"Reachable states: {reachableStates.Count}\nUnreachable states: {unreachableCount}";
            EditorUtility.DisplayDialog("Validation Complete", message, "OK");
        }

        private HashSet<SerializedUnitState> FindReachableStates(SerializedUnitState entryState)
        {
            var reachable = new HashSet<SerializedUnitState>();
            var queue = new Queue<SerializedUnitState>();

            queue.Enqueue(entryState);
            reachable.Add(entryState);

            while (queue.Count > 0)
            {
                var currentState = queue.Dequeue();

                // Find all transitions from this state
                foreach (var transition in currentState.transitions)
                {
                    if (transition.nextState != null && !reachable.Contains(transition.nextState))
                    {
                        reachable.Add(transition.nextState);
                        queue.Enqueue(transition.nextState);
                    }
                }
            }

            return reachable;
        }
    }
}