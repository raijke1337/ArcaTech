using System.Text;
using UnityEditor;
using UnityEngine;

namespace Arcatech.Units.Editor
{
    [CustomEditor(typeof(EntityStateMachineComponent))]
    public class EntityStateMachineComponentEditor : UnityEditor.Editor
    {
        SerializedProperty gameEntityProp;
        SerializedProperty animatorProp;
        SerializedProperty startingStateProp;
        SerializedProperty verboseProp;

        void OnEnable()
        {
            gameEntityProp   = serializedObject.FindProperty("gameEntity");
            animatorProp     = serializedObject.FindProperty("animator");
            startingStateProp = serializedObject.FindProperty("startingState");
            verboseProp      = serializedObject.FindProperty("verboseDebugs");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(gameEntityProp);
            EditorGUILayout.PropertyField(animatorProp);
            EditorGUILayout.PropertyField(startingStateProp);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(verboseProp, new GUIContent("Verbose Debugs"));

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            DrawRuntimeInfo((EntityStateMachineComponent)target);

            if (Application.isPlaying)
            {
                EditorGUILayout.Space();
                DrawRuntimeControls((EntityStateMachineComponent)target);
            }
        }

        void DrawRuntimeInfo(EntityStateMachineComponent machine)
        {
            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                EditorGUILayout.LabelField("Runtime State", EditorStyles.boldLabel);

                if (!Application.isPlaying)
                {
                    EditorGUILayout.HelpBox("Enter Play Mode to view runtime state info.", MessageType.Info);
                    return;
                }

                var ctx = machine.Context;
                var currentState = ctx?.CurrentState?.StateName ?? "(null)";
                var pending = ctx?.PendingCommand.ToString() ?? "None";
                var pendingPerformers = machine.PendingPerformersCount;
                var canExit = machine.CurrentUnitState?.CanExitState(machine.Animator) ?? false;

                EditorGUILayout.LabelField("Current State", currentState);
                EditorGUILayout.LabelField("Time In State", machine.CurrentUnitState?.TimeInState.ToString("F2") ?? "0.00");
                EditorGUILayout.LabelField("Animator Layer", machine.CurrentUnitState?.AnimatorLayer.ToString() ?? "-");
                EditorGUILayout.LabelField("Can Exit", canExit ? "Yes" : "No");
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Pending Command", pending);
                EditorGUILayout.LabelField("Command Performers", pendingPerformers.ToString());

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Pause / Death");
                EditorGUILayout.Toggle("Paused", machine.Paused);
            //    EditorGUILayout.Toggle("Killed", machine.SetKilled(null,true));

                EditorGUILayout.Space();
                DrawAugmentorList(machine);
                DrawCandidateList(machine);
            }
        }

        void DrawAugmentorList(EntityStateMachineComponent machine)
        {
            var augmentors = machine.ActiveAugmentors;
            EditorGUILayout.LabelField($"Active Augmentors ({augmentors.Count})", EditorStyles.boldLabel);

            if (augmentors.Count == 0)
            {
                EditorGUILayout.HelpBox("No augmentors registered.", MessageType.None);
                return;
            }

            foreach (var aug in augmentors)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(aug?.GetType().Name ?? "(null)");
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeObject = aug as UnityEngine.Object;
                }
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    machine.UnregisterAugmentor(aug);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        void DrawCandidateList(EntityStateMachineComponent machine)
        {
            var candidates = machine.DebugCandidates;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Last Candidates ({candidates.Count})", EditorStyles.boldLabel);

            if (candidates.Count == 0)
            {
                EditorGUILayout.HelpBox("No candidates cached yet. They populate after Update runs.", MessageType.None);
                return;
            }

            foreach (var info in machine.DebugCandidates)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(info.Source, EditorStyles.miniBoldLabel);
                    EditorGUILayout.LabelField("Next", info.NextState);
                    EditorGUILayout.LabelField("Priority", info.Priority.ToString());
                    EditorGUILayout.LabelField("Exit Norm", info.ExitNormalizedTime.ToString("F2"));
                    EditorGUILayout.LabelField("Override Min Time", info.OverrideMinTime ? "Yes" : "No");
                    EditorGUILayout.LabelField("Can Transition", info.CanTransition ? "Yes" : "No");
                }
            }
        }

        void DrawRuntimeControls(EntityStateMachineComponent machine)
        {
            EditorGUILayout.LabelField("Debug Controls", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Force Refresh Candidates"))
                    machine.DebugRefreshCandidates();

                if (GUILayout.Button("Clear Pending Command"))
                    machine.DebugClearPendingCommand();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Toggle Pause"))
                    machine.Paused = !machine.Paused;

                // if (GUILayout.Button("Toggle Killed"))
                //     machine.SetKilled(!machine.Killed);
            }
        }
    }
}