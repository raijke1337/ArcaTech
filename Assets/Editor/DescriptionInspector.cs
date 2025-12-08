using Arcatech.Texts;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Description))]
public class DescriptionInspector : Editor
{
    private GUIStyle _cardStyle;
    private GUIStyle _titleStyle;
    private GUIStyle _labelStyle;

    private void OnEnable()
    {
        _cardStyle = new GUIStyle("box")
        {
            padding = new RectOffset(16, 16, 16, 16),
            margin = new RectOffset(0, 0, 10, 10)
        };

        _titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleCenter
        };

        _labelStyle = new GUIStyle(EditorStyles.label)
        {
            fontStyle = FontStyle.Bold
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var titleProp = serializedObject.FindProperty("Title");
        var textProp = serializedObject.FindProperty("Text");
        var pictureProp = serializedObject.FindProperty("Picture");
        var flavorProp = serializedObject.FindProperty("FlavorText");

        EditorGUILayout.BeginVertical(_cardStyle);

        EditorGUILayout.PropertyField(titleProp, new GUIContent("Title"));
        if (!string.IsNullOrWhiteSpace(titleProp.stringValue))
        {
            GUILayout.Label(titleProp.stringValue, _titleStyle);
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Picture", _labelStyle);
        var sprite = pictureProp.objectReferenceValue as Sprite;
        if (sprite != null)
        {
            GUILayout.Label(AssetPreview.GetAssetPreview(sprite), GUILayout.Height(120), GUILayout.ExpandWidth(true));
        }

        EditorGUILayout.PropertyField(pictureProp, GUIContent.none);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Description", _labelStyle);
        textProp.stringValue = EditorGUILayout.TextArea(textProp.stringValue, GUILayout.Height(90));

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Flavor Text", _labelStyle);
        flavorProp.stringValue = EditorGUILayout.TextArea(flavorProp.stringValue, GUILayout.Height(60));

        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}