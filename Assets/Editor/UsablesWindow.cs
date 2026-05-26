
using System.Collections.Generic;
using Arcatech.Usables;
using UnityEditor;
using UnityEngine;

public class SerializedUsableStrategiesWindow : EditorWindow
{
    private Vector2 scrollPos;
    private List<SerializedUsableStrategy> strategies;
    private const int Columns = 3;  // Adjust number of columns as needed
    private const float CardWidth = 200f;  // Adjust card width

    [MenuItem("Window/Game/Usables")]
    private static void ShowWindow()
    {
        var window = GetWindow<SerializedUsableStrategiesWindow>();
        window.titleContent = new GUIContent("Usable Strategies");
        window.Show();
    }

    private void OnEnable()
    {
        LoadStrategies();
    }

    private void LoadStrategies()
    {
        strategies = new List<SerializedUsableStrategy>();
        string[] guids = AssetDatabase.FindAssets("t:SerializedUsableStrategy");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SerializedUsableStrategy strategy = AssetDatabase.LoadAssetAtPath<SerializedUsableStrategy>(path);
            if (strategy != null)
            {
                strategies.Add(strategy);
            }
        }
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Refresh"))
        {
            LoadStrategies();
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        int itemCount = strategies.Count;
        int rows = Mathf.CeilToInt((float)itemCount / Columns);
        
        for (int row = 0; row < rows; row++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int col = 0; col < Columns; col++)
            {
                int index = row * Columns + col;
                if (index < itemCount)
                {
                    DrawStrategyCard(strategies[index]);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawStrategyCard(SerializedUsableStrategy strategy)
    {
        EditorGUILayout.BeginVertical("box", GUILayout.Width(CardWidth));
        
        if (strategy.description != null)
        {
            // Display icon
            if (strategy.description.Picture != null)
            {
                Texture2D texture = strategy.description.Picture.texture;
                if (texture != null)
                {
                    Rect rect = GUILayoutUtility.GetRect(CardWidth - 10, 64f, 64f, 64f, GUILayout.ExpandWidth(false));
                    GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit);
                }
            }
            
            // Display title
            EditorGUILayout.LabelField(strategy.description.Title, EditorStyles.boldLabel);
            
            // Display flavor text or text as description
            string desc = !string.IsNullOrEmpty(strategy.description.FlavorText) ? strategy.description.FlavorText : strategy.description.Text;
            EditorGUILayout.LabelField(desc, EditorStyles.wordWrappedLabel);
        }
        else
        {
            EditorGUILayout.LabelField("No Description", EditorStyles.boldLabel);
        }
        
        // Additional info
        if (strategy.usableData != null)
        {
            EditorGUILayout.LabelField("Effects: " + strategy.usableData.Length);
        }
        
        if (strategy.settings.useCost != null)
        {
            EditorGUILayout.LabelField("Use Cost: Present"); // Adjust to display more info if needed
        }
        
        if (GUILayout.Button("Select"))
        {
            Selection.activeObject = strategy;
        }
        
        EditorGUILayout.EndVertical();
    }
}