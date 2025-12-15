using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Arcatech.Items.Editor
{

    [CustomEditor(typeof(UsablesCasterComponent))]
    public class UsablesCasterComponentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw default inspector first
            DrawDefaultInspector();
        
            UsablesCasterComponent caster = (UsablesCasterComponent)target;
        
            if (caster.GetUsables == null || caster.GetUsables.Count == 0)
            {
                EditorGUILayout.HelpBox("No usable items found.", MessageType.Info);
                return;
            }
        
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Usable Items", EditorStyles.boldLabel);
        
            // Table header
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Item Name", GUILayout.Width(120));
            EditorGUILayout.LabelField("Charges", GUILayout.Width(60));
            EditorGUILayout.LabelField("Recharge Timer", GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();
        
            // Draw separator
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, Color.gray);
        
            // Display each usable item as a row
            for (int i = 0; i < caster.GetUsables.Count; i++)
            {
                IUsable usable = caster.GetUsables.Values.ToArray()[i];
                if (usable == null) continue;
            
                EditorGUILayout.BeginHorizontal();
            
                // Item name
                EditorGUILayout.LabelField(usable.Description.Title ?? "Unnamed", GUILayout.Width(120));
            
                // Charges
                EditorGUILayout.LabelField(usable.StringInfo, GUILayout.Width(60));
            
                // Recharge timer
                EditorGUILayout.LabelField(usable.FillValue.ToString("F1"), GUILayout.Width(100));
            
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}