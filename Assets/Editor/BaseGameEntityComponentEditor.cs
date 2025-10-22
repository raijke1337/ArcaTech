using UnityEngine;
using UnityEditor;
using System.Linq;
using Arcatech.Units;

namespace Arcatech
{
    [CustomEditor(typeof(BaseGameEntityComponent))]
    public class BaseGameEntityComponentEditor : Editor
    { 
        
        private SerializedProperty effectsTakersProperty;
    private bool showEffectsTakersList = true;
    private Vector2 scrollPosition;

    private void OnEnable()
    {
        // Find the serialized property for the effects takers list
        effectsTakersProperty = serializedObject.FindProperty("effectsTakers");
    }

    public override void OnInspectorGUI()
    {
        // Update the serialized object
        serializedObject.Update();
        
        // Draw the default inspector
        DrawDefaultInspector();
        
        // Add some space
        EditorGUILayout.Space(10);
        
        // Draw the custom effects takers section
        DrawEffectsTakersSection();
        
        // Apply any changes
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawEffectsTakersSection()
    {
        BaseGameEntityComponent baseEntity = (BaseGameEntityComponent)target;
        
        // Create a foldout for the effects takers list
        EditorGUILayout.BeginVertical("box");
        
        showEffectsTakersList = EditorGUILayout.Foldout(showEffectsTakersList, 
            $"Effects Takers ({baseEntity.GetEffectsTakersForEditor?.Count ?? 0})", true);
        
        if (showEffectsTakersList)
        {
            EditorGUI.indentLevel++;
            
            if (baseEntity.GetEffectsTakersForEditor == null || baseEntity.GetEffectsTakersForEditor.Count == 0)
            {
                EditorGUILayout.HelpBox("No Effects Takers assigned.", MessageType.Info);
            }
            else
            {
                // Start scroll view for large lists
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(200));
                
                // Draw table header
                DrawTableHeader();
                
                // Draw each effects taker
                for (int i = 0; i < baseEntity.GetEffectsTakersForEditor.Count; i++)
                {
                    DrawEffectsTakerRow(baseEntity.GetEffectsTakersForEditor[i], i);
                }
                
                EditorGUILayout.EndScrollView();
            }
            
            // Add/Remove buttons
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.EndVertical();
    }

    private void DrawTableHeader()
    {
        EditorGUILayout.BeginHorizontal();
        
        // Index column
        EditorGUILayout.LabelField("", GUILayout.Width(30));
        
        // Component name column
        EditorGUILayout.LabelField("Component", EditorStyles.boldLabel, GUILayout.Width(150));
        
        // GameObject name column
        EditorGUILayout.LabelField("GameObject", EditorStyles.boldLabel, GUILayout.Width(120));
        
        // Type column
        EditorGUILayout.LabelField("Type", EditorStyles.boldLabel);
        
        EditorGUILayout.EndHorizontal();
        
        // Draw separator line
        Rect rect = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(rect, Color.gray);
    }

    private void DrawEffectsTakerRow(IEffectsTakerComponent effectsTaker, int index)
    {
        EditorGUILayout.BeginHorizontal();
        
        // Index
        EditorGUILayout.LabelField(index.ToString(), GUILayout.Width(30));
        
        if (effectsTaker == null)
        {
            // Null reference
            EditorGUILayout.LabelField("NULL", EditorStyles.helpBox, GUILayout.Width(150));
            EditorGUILayout.LabelField("-", GUILayout.Width(120));
            EditorGUILayout.LabelField("-");
        }
        else
        {
            // Get component info
            string componentName = GetComponentDisplayName(effectsTaker);
            string gameObjectName = GetGameObjectName(effectsTaker);
            string typeName = effectsTaker.GetType().Name;
            
            // Component name (clickable if it's a MonoBehaviour)
            if (effectsTaker is MonoBehaviour monoBehaviour)
            {
                if (GUILayout.Button(componentName, EditorStyles.linkLabel, GUILayout.Width(150)))
                {
                    // Ping the component in hierarchy
                    EditorGUIUtility.PingObject(monoBehaviour);
                    Selection.activeObject = monoBehaviour;
                }
            }
            else
            {
                EditorGUILayout.LabelField(componentName, GUILayout.Width(150));
            }
            
            // GameObject name (clickable)
            if (effectsTaker is MonoBehaviour mb && mb != null)
            {
                if (GUILayout.Button(gameObjectName, EditorStyles.linkLabel, GUILayout.Width(120)))
                {
                    EditorGUIUtility.PingObject(mb.gameObject);
                    Selection.activeGameObject = mb.gameObject;
                }
            }
            else
            {
                EditorGUILayout.LabelField(gameObjectName, GUILayout.Width(120));
            }
            
            // Type name
            EditorGUILayout.LabelField(typeName);
        }
        
        EditorGUILayout.EndHorizontal();
    }



    /*private void ShowAddEffectsTakerMenu(BaseEntity baseEntity)
    {
        GenericMenu menu = new GenericMenu();
        
        // Find all IEffectsTaker components in the scene
        var allEffectsTakers = FindObjectsOfType<MonoBehaviour>()
            .Where(mb => mb is IEffectsTaker)
            .Cast<IEffectsTaker>()
            .ToList();
        
        if (allEffectsTakers.Count == 0)
        {
            menu.AddDisabledItem(new GUIContent("No IEffectsTaker components found"));
        }
        else
        {
            foreach (var effectsTaker in allEffectsTakers)
            {
                string menuPath = GetMenuPath(effectsTaker);
                bool isAlreadyAdded = baseEntity.effectsTakers.Contains(effectsTaker);
                
                if (isAlreadyAdded)
                {
                    menu.AddDisabledItem(new GUIContent(menuPath + " (Already added)"));
                }
                else
                {
                    menu.AddItem(new GUIContent(menuPath), false, () => AddEffectsTaker(baseEntity, effectsTaker));
                }
            }
        }
        
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Refresh List"), false, () => Repaint());
        
        menu.ShowAsContext();
    }*/


    private string GetComponentDisplayName(IEffectsTakerComponent effectsTaker)
    {
        if (effectsTaker == null) return "NULL";
        
        if (effectsTaker is MonoBehaviour monoBehaviour)
        {
            return monoBehaviour.GetType().Name;
        }
        
        return effectsTaker.GetType().Name;
    }

    private string GetGameObjectName(IEffectsTakerComponent effectsTaker)
    {
        if (effectsTaker == null) return "-";
        
        if (effectsTaker is MonoBehaviour monoBehaviour && monoBehaviour != null)
        {
            return monoBehaviour.gameObject.name;
        }
        
        return "Unknown";
    }

    private string GetMenuPath(IEffectsTakerComponent effectsTaker)
    {
        if (effectsTaker is MonoBehaviour monoBehaviour && monoBehaviour != null)
        {
            return $"{monoBehaviour.gameObject.name}/{monoBehaviour.GetType().Name}";
        }
        
        return effectsTaker.GetType().Name;
    }
    }
}