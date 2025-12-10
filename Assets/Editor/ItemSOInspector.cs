// Place in Editor folder
// File: ItemSOEditor.cs

using Arcatech.Items;
using UnityEngine;
using UnityEditor;
using Arcatech.Texts;
using Arcatech.Texts.Editor;
using Arcatech.Usables;

[CustomEditor(typeof(ItemSO), true)]
[CanEditMultipleObjects]
public class ItemSOEditor : Editor
{
    // Foldout states
    private bool showDescription = true;
    private bool showItemSettings = true;
    private bool showEquipmentSettings = true;
    private bool showUsablesSettings = true;
    
    // Properties
    private SerializedProperty descriptionProp;
    private SerializedProperty maxStackProp;
    private SerializedProperty worldItemContainerPrefabProp;
    private SerializedProperty itemPrefabProp;
    private SerializedProperty slotProp;
    private SerializedProperty statModifiersProp;
    private SerializedProperty periodicDeltasProp;
    private SerializedProperty usedActionsProp;
    
    // Cached editor for the Description ScriptableObject
    private Editor descriptionEditor;
    private Description cachedDescription;
    
    private GUIStyle boxStyle;
    
    private void OnEnable()
    {
        // Cache all properties
        descriptionProp = serializedObject.FindProperty("description");
        maxStackProp = serializedObject.FindProperty("MaxStack");
        worldItemContainerPrefabProp = serializedObject.FindProperty("worldItemContainerPrefab");
        
        // EquipSO properties
        itemPrefabProp = serializedObject.FindProperty("itemPrefab");
        slotProp = serializedObject.FindProperty("slot");
        statModifiersProp = serializedObject.FindProperty("statModifiers");
        periodicDeltasProp = serializedObject.FindProperty("periodicDeltas");
        
        // UsablesSO properties
        usedActionsProp = serializedObject.FindProperty("usedActions");
        
        // Cache description editor
        UpdateDescriptionEditor();
    }
    
    private void OnDisable()
    {
        // Clean up the cached editor
        if (descriptionEditor != null)
        {
            DestroyImmediate(descriptionEditor);
        }
    }
    
    private void UpdateDescriptionEditor()
    {
        Description newDescription = descriptionProp?.objectReferenceValue as Description;
        
        if (newDescription != cachedDescription)
        {
            cachedDescription = newDescription;
            
            if (descriptionEditor != null)
            {
                DestroyImmediate(descriptionEditor);
            }
            
            if (cachedDescription != null)
            {
                descriptionEditor = Editor.CreateEditor(cachedDescription);
            }
        }
    }
    
    private void InitStyles()
    {
        if (boxStyle == null)
        {
            boxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };
        }
    }

    public override void OnInspectorGUI()
    {
        InitStyles();
        serializedObject.Update();
        
        // Header with type indicator
        DrawTypeHeader();
        
        EditorGUILayout.Space(10);
        
        // Description Section
        DrawDescriptionSection();
        
        // Item Settings Section
        DrawItemSettingsSection();
        
        // Equipment Section (if applicable)
        if (target is EquipSO)
        {
            DrawEquipmentSection();
        }
        
        // Usables Section (if applicable)
        if (target is UsablesSO)
        {
            DrawUsablesSection();
        }
        
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawTypeHeader()
    {
        string typeName = target.GetType().Name;
        Color headerColor = GetTypeColor();
        
        EditorGUILayout.BeginHorizontal();
        
        // Colored bar
        Rect colorRect = GUILayoutUtility.GetRect(4, 24, GUILayout.Width(4));
        EditorGUI.DrawRect(colorRect, headerColor);
        
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 };
        EditorGUILayout.LabelField(typeName, headerStyle);
        
        EditorGUILayout.EndHorizontal();
        
        // Separator
        Rect lineRect = GUILayoutUtility.GetRect(1, 1);
        EditorGUI.DrawRect(lineRect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
    }

    private Color GetTypeColor()
    {
        if (target is UsablesSO) return new Color(0.4f, 0.8f, 0.4f);
        if (target is EquipSO) return new Color(0.4f, 0.6f, 0.9f);
        return new Color(0.9f, 0.7f, 0.3f);
    }

    private void DrawDescriptionSection()
    {
        showDescription = EditorGUILayout.Foldout(showDescription, "Description", true, EditorStyles.foldoutHeader);
        
        if (showDescription)
        {
            EditorGUILayout.Space(5);
            
            // Description object field
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(descriptionProp, new GUIContent("Description Asset"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                UpdateDescriptionEditor();
            }
            
            EditorGUILayout.Space(5);
            
            // Draw the description card if we have a reference
            Description description = descriptionProp.objectReferenceValue as Description;
            
            if (description != null)
            {
                // Option 1: Read-only preview
                // DescriptionEditor.DrawDescriptionCard(description);
                
                // Option 2: Editable inline (edits the Description asset directly)
                using (new EditorGUILayout.VerticalScope(boxStyle))
                {
                    EditorGUILayout.LabelField("Edit Description", EditorStyles.boldLabel);
                    EditorGUILayout.Space(5);
                    
                    SerializedObject descSO = new SerializedObject(description);
                    DescriptionEditor.DrawEditableDescriptionCard(descSO);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Assign a Description asset to see the card preview.", MessageType.Info);
                
                // Quick create button
                if (GUILayout.Button("Create New Description Asset"))
                {
                    CreateNewDescriptionAsset();
                }
            }
            
            EditorGUILayout.Space(5);
        }
    }
    
    private void CreateNewDescriptionAsset()
    {
        // Get the path of the current item asset
        string itemPath = AssetDatabase.GetAssetPath(target);
        string directory = System.IO.Path.GetDirectoryName(itemPath);
        string itemName = target.name;
        
        // Create a new Description asset
        Description newDescription = ScriptableObject.CreateInstance<Description>();
        newDescription.Title = itemName;
        
        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{directory}/{itemName}_Description.asset");
        AssetDatabase.CreateAsset(newDescription, assetPath);
        AssetDatabase.SaveAssets();
        
        // Assign it to the item
        descriptionProp.objectReferenceValue = newDescription;
        serializedObject.ApplyModifiedProperties();
        
        UpdateDescriptionEditor();
        
        // Ping the new asset
        EditorGUIUtility.PingObject(newDescription);
    }

    private void DrawItemSettingsSection()
    {
        EditorGUILayout.Space(5);
        showItemSettings = EditorGUILayout.Foldout(showItemSettings, "Item Settings", true, EditorStyles.foldoutHeader);
        
        if (showItemSettings)
        {
            using (new EditorGUILayout.VerticalScope(boxStyle))
            {
                if (maxStackProp != null)
                {
                    EditorGUILayout.IntSlider(maxStackProp, 1, 999, new GUIContent("Max Stack"));
                }
                
                EditorGUILayout.Space(5);
                
                if (worldItemContainerPrefabProp != null)
                {
                    EditorGUILayout.PropertyField(worldItemContainerPrefabProp, 
                        new GUIContent("World Item Prefab"));
                }
            }
        }
    }

    private void DrawEquipmentSection()
    {
        EditorGUILayout.Space(5);
        showEquipmentSettings = EditorGUILayout.Foldout(showEquipmentSettings, "Equipment Settings", true, EditorStyles.foldoutHeader);
        
        if (showEquipmentSettings)
        {
            using (new EditorGUILayout.VerticalScope(boxStyle))
            {
                if (itemPrefabProp != null)
                {
                    EditorGUILayout.PropertyField(itemPrefabProp, new GUIContent("Equipment Prefab"));
                }
                
                if (slotProp != null)
                {
                    EditorGUILayout.PropertyField(slotProp, new GUIContent("Equipment Slot"));
                }
                
                EditorGUILayout.Space(10);
                
                if (statModifiersProp != null)
                {
                    EditorGUILayout.PropertyField(statModifiersProp, new GUIContent("Stat Modifiers"), true);
                }
                
                EditorGUILayout.Space(5);
                
                if (periodicDeltasProp != null)
                {
                    EditorGUILayout.PropertyField(periodicDeltasProp, new GUIContent("Periodic Deltas"), true);
                }
            }
        }
    }

    private void DrawUsablesSection()
    {
        EditorGUILayout.Space(5);
        showUsablesSettings = EditorGUILayout.Foldout(showUsablesSettings, "Usable Actions", true, EditorStyles.foldoutHeader);
        
        if (showUsablesSettings)
        {
            using (new EditorGUILayout.VerticalScope(boxStyle))
            {
                if (usedActionsProp != null)
                {
                    EditorGUILayout.PropertyField(usedActionsProp, new GUIContent("Action Mappings"), true);
                }
            }
        }
    }
}