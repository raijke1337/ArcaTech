// File: Assets/Editor/EntityStateMachineInspector.cs
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Arcatech.Units;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EntityStateMachineComponent))]
public class EntityStateMachineInspector : Editor
{
    // Foldout states for transitions: key by a transition id (hash)
    private Dictionary<int, bool> _transitionFoldouts = new Dictionary<int, bool>();

    // Cached reflection info
    private FieldInfo _currentStateField;
    private FieldInfo _contextField;
    private FieldInfo _addedTransitionsField;
    private FieldInfo _animatorFieldInfo;

    // GUIStyles for colored badges
    private GUIStyle _trueStyle;
    private GUIStyle _falseStyle;
    private GUIStyle _conditionLabelStyle;

    void OnEnable()
    {
        var t = typeof(EntityStateMachineComponent);
        _currentStateField = t.GetField("_currentState", BindingFlags.NonPublic | BindingFlags.Instance);
        _contextField = t.GetField("_context", BindingFlags.NonPublic | BindingFlags.Instance);
        _addedTransitionsField = t.GetField("_addedTransitions", BindingFlags.NonPublic | BindingFlags.Instance);
        _animatorFieldInfo = t.GetField("animator", BindingFlags.NonPublic | BindingFlags.Instance);

        // Setup styles
        _trueStyle = new GUIStyle(EditorStyles.label) { richText = true };
        _falseStyle = new GUIStyle(EditorStyles.label) { richText = true };
        _conditionLabelStyle = new GUIStyle(EditorStyles.label) { richText = true };

        _trueStyle.normal.textColor = Color.green;
        _falseStyle.normal.textColor = Color.red;
        _conditionLabelStyle.richText = true;
    }

    public override void OnInspectorGUI()
    {
        // Draw the default inspector first so serialized fields remain editable.
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Runtime State Machine Debug", EditorStyles.boldLabel);

        var comp = (EntityStateMachineComponent)target;

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play mode to see runtime state, transitions and live condition evaluation and to use Force Transition.", MessageType.Info);
            // Show starting state reference
            var startProp = serializedObject.FindProperty("startingState");
            if (startProp != null)
            {
                EditorGUILayout.PropertyField(startProp);
            }
            return;
        }

        // Get runtime values via reflection
        var currentState = _currentStateField?.GetValue(comp);
        var ctx = _contextField?.GetValue(comp);
        var addedTransitions = _addedTransitionsField?.GetValue(comp) as IEnumerable<StateTransition>;
        var animator = _animatorFieldInfo?.GetValue(comp) as Animator;

        // Current State display
        if (currentState == null)
        {
            EditorGUILayout.LabelField("Current State:", "(null)");
        }
        else
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Current State:", EditorStyles.boldLabel);

            string stateName = GetMemberString(currentState, "Name") ?? currentState.ToString();
            EditorGUILayout.LabelField("Name", stateName);

            float timeInState = GetMemberFloat(currentState, "TimeInState");
            if (timeInState >= 0f)
                EditorGUILayout.LabelField("Time in State", timeInState.ToString("F2") + "s");

            if (animator != null)
                EditorGUILayout.LabelField("Animator", animator.name);

            EditorGUILayout.EndVertical();
        }

        // New: display PendingCommand and PendingCommandData from context
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Pending Command", EditorStyles.boldLabel);
        if (ctx == null)
        {
            EditorGUILayout.LabelField("(Context not available)");
        }
        else
        {
            string pending = GetMemberObjectString(ctx, "PendingCommand") ?? "(None)";
            string pendingData = GetMemberObjectString(ctx, "PendingCommandData") ?? "(None)";
            EditorGUILayout.LabelField("PendingCommand", pending);
            EditorGUILayout.LabelField("PendingCommandData", pendingData);
        }

        EditorGUILayout.Space();

        // Global (added) transitions
        if (addedTransitions != null && addedTransitions.Any())
        {
            EditorGUILayout.LabelField("Registered (Global) Transitions", EditorStyles.boldLabel);
            foreach (var t in addedTransitions.OrderByDescending(x => x.TransitionPriority))
            {
                DrawTransitionEntry(t, ctx, currentState, animator, comp);
            }
            EditorGUILayout.Space();
        }

        // Current state transitions
        if (currentState != null)
        {
            var transitions = GetMemberArray(currentState, "Transitions") as IEnumerable<object>;
            if (transitions == null)
            {
                EditorGUILayout.HelpBox("Could not find transitions on the runtime UnitState instance. Make sure UnitState exposes 'Transitions'.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField("Current State Transitions", EditorStyles.boldLabel);
                foreach (var trObj in transitions)
                {
                    var tr = trObj as StateTransition;
                    DrawTransitionEntry(tr, ctx, currentState, animator, comp);
                }
            }
        }

        // Ping startingState asset quick button
        EditorGUILayout.Space();
        if (GUILayout.Button("Ping startingState asset"))
        {
            var startProp = serializedObject.FindProperty("startingState");
            if (startProp != null && startProp.objectReferenceValue != null)
            {
                EditorGUIUtility.PingObject(startProp.objectReferenceValue);
            }
        }
    }

    private void DrawTransitionEntry(StateTransition tr, object ctx, object currentStateObj, Animator animatorField, EntityStateMachineComponent comp)
    {
        if (tr == null) return;
        int id = tr.GetHashCode();

        // Default expanded by default: if not tracked, set true
        if (!_transitionFoldouts.ContainsKey(id))
            _transitionFoldouts[id] = true;

        bool fold = _transitionFoldouts[id];
        // Make the header a bold label (no small arrow needed)
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Transition -> {(tr.NextState != null ? GetMemberString(tr.NextState, "Name") : "(null)")}", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Priority {tr.TransitionPriority}", GUILayout.Width(90));
        EditorGUILayout.EndHorizontal();

        // Keep it always expanded (but allow collapse toggle)
        bool newFold = EditorGUILayout.ToggleLeft("Expanded", fold);
        _transitionFoldouts[id] = newFold;
        EditorGUILayout.Space();

        if (newFold)
        {
            // Next State
            if (tr.NextState != null)
                EditorGUILayout.LabelField("Next State", GetMemberString(tr.NextState, "Name") ?? tr.NextState.ToString());
            else
                EditorGUILayout.LabelField("Next State", "(null)");

            // Exit Normalized Time
            float exitNorm = GetMemberFloat(tr, "ExitNormalizedTime");
            EditorGUILayout.LabelField("Exit Normalized Time", exitNorm.ToString("F2"));

            // Exit time passed? call currentState.ExitTimePassed if exists
            bool passedExit = false;
            if (currentStateObj != null)
            {
                MethodInfo exitCheck = currentStateObj.GetType().GetMethod("ExitTimePassed", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (exitCheck != null)
                {
                    try
                    {
                        var result = exitCheck.Invoke(currentStateObj, new object[] { animatorField, exitNorm });
                        if (result is bool b) passedExit = b;
                    }
                    catch { passedExit = false; }
                }
            }
            EditorGUILayout.LabelField("Exit Time Passed", passedExit ? "Yes" : "No");

            EditorGUILayout.Space();

            // Conditions - show each with green/red badge
            var conds = GetMemberArray(tr, "Conditions") as IEnumerable<object>;
            if (conds == null)
                conds = GetMemberArray(tr, "conditions") as IEnumerable<object>;

            EditorGUILayout.LabelField("Conditions", EditorStyles.boldLabel);
            if (conds == null)
            {
                EditorGUILayout.LabelField("(no conditions found)");
            }
            else
            {
                foreach (var c in conds)
                {
                    if (c == null)
                    {
                        EditorGUILayout.LabelField("- (null)");
                        continue;
                    }

                    // Evaluate the condition's CanTransition(ctx)
                    bool condValue = false;
                    try
                    {
                        MethodInfo canTransition = c.GetType().GetMethod("CanTransition", BindingFlags.Public | BindingFlags.Instance);
                        if (canTransition != null && ctx != null)
                        {
                            var val = canTransition.Invoke(c, new object[] { ctx });
                            if (val is bool vb) condValue = vb;
                        }
                    }
                    catch (Exception ex)
                    {
                        condValue = false;
                        Debug.LogException(ex);
                    }

                    // Condition label and badge
                    string condLabel = c is UnityEngine.Object uo ? uo.name : c.GetType().Name;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("- " + condLabel, GUILayout.MaxWidth(280));

                    // Colored badge using rich text
                    var badgeStyle = condValue ? _trueStyle : _falseStyle;
                    string badgeText = condValue ? "<b>True</b>" : "<b>False</b>";
                    GUILayout.Label(badgeText, badgeStyle, GUILayout.Width(48));
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.Space();

            // OnTransition actions listing
            var actions = GetMemberArray(tr, "OnTransition") as IEnumerable<object>;
            if (actions == null) actions = GetMemberArray(tr, "onTransition") as IEnumerable<object>;
            EditorGUILayout.LabelField("OnTransition Actions", EditorStyles.boldLabel);
            if (actions == null)
            {
                EditorGUILayout.LabelField("(none)");
            }
            else
            {
                foreach (var a in actions)
                {
                    if (a == null) { EditorGUILayout.LabelField("- (null)"); continue; }
                    string name = a is UnityEngine.Object uo ? uo.name : a.GetType().Name;
                    EditorGUILayout.LabelField("- " + name);
                }
            }

            EditorGUILayout.Space();

            // Force Transition button (Play mode only and only if NextState exists)
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(!Application.isPlaying || tr.NextState == null);
            if (GUILayout.Button("Force Transition", GUILayout.Width(140)))
            {
                if (tr.NextState != null)
                {
                    // Call comp.ForceUnitState(tr.NextState, true)
                    comp.ForceUnitState(tr.NextState, true);
                }
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    // Helper: get a string property or field
    private string GetMemberString(object obj, string memberName)
    {
        if (obj == null) return null;
        var t = obj.GetType();
        var prop = t.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (prop != null && prop.PropertyType == typeof(string))
        {
            return prop.GetValue(obj) as string;
        }
        var field = t.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null && field.FieldType == typeof(string))
        {
            return field.GetValue(obj) as string;
        }
        return null;
    }

    // Helper: get float property or field (returns -1 if not found)
    private float GetMemberFloat(object obj, string memberName)
    {
        if (obj == null) return -1f;
        var t = obj.GetType();
        var prop = t.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (prop != null && (prop.PropertyType == typeof(float) || prop.PropertyType == typeof(double)))
        {
            var v = prop.GetValue(obj);
            if (v is float f) return f;
            if (v is double d) return (float)d;
        }
        var field = t.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null && (field.FieldType == typeof(float) || field.FieldType == typeof(double)))
        {
            var v = field.GetValue(obj);
            if (v is float f) return f;
            if (v is double d) return (float)d;
        }
        return -1f;
    }

    // Helper: get array/collection property/field
    private IEnumerable<object> GetMemberArray(object obj, string memberName)
    {
        if (obj == null) return null;
        var t = obj.GetType();
        var prop = t.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (prop != null)
        {
            var val = prop.GetValue(obj);
            if (val is System.Collections.IEnumerable ie) return ie.Cast<object>();
        }
        var field = t.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            var val = field.GetValue(obj);
            if (val is System.Collections.IEnumerable ie) return ie.Cast<object>();
        }
        return null;
    }

    // New helper: return object's member value as string (handles enums and null)
    private string GetMemberObjectString(object obj, string memberName)
    {
        if (obj == null) return null;
        var t = obj.GetType();
        var prop = t.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        object val = null;
        if (prop != null) val = prop.GetValue(obj);
        else
        {
            var field = t.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null) val = field.GetValue(obj);
        }
        if (val == null) return null;
        return val.ToString();
    }
}
#endif