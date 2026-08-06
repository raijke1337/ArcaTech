using Arcatech.UI;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(TerminalButton), true)]
[CanEditMultipleObjects]
public class TerminalButtonEditor : ButtonEditor
{
    private SerializedProperty _background;
    private SerializedProperty _frame;
    private SerializedProperty _bgNormal;
    private SerializedProperty _bgHover;
    private SerializedProperty _bgPressed;
    private SerializedProperty _frameNormal;
    private SerializedProperty _frameHover;
    private SerializedProperty _framePressed;
    private SerializedProperty _fadeDuration;

    protected override void OnEnable()
    {
        base.OnEnable();
        
        _background    = serializedObject.FindProperty("_background");
        _frame         = serializedObject.FindProperty("_frame");
        _bgNormal      = serializedObject.FindProperty("_bgNormal");
        _bgHover       = serializedObject.FindProperty("_bgHover");
        _bgPressed     = serializedObject.FindProperty("_bgPressed");
        _frameNormal   = serializedObject.FindProperty("_frameNormal");
        _frameHover    = serializedObject.FindProperty("_frameHover");
        _framePressed  = serializedObject.FindProperty("_framePressed");
        _fadeDuration  = serializedObject.FindProperty("_fadeDuration");
    }

    public override void OnInspectorGUI()
    {
        // Сначала рисуем стандартные поля Button 
        // (Interactable, Transition, Navigation, OnClick)
        base.OnInspectorGUI();
        
        EditorGUILayout.Space(10);
        
        serializedObject.Update();
        
        // === Visual Elements ===
        EditorGUILayout.LabelField("Visual Elements", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_background);
        EditorGUILayout.PropertyField(_frame);
        
        EditorGUILayout.Space(5);
        
        // === Background Colors ===
        EditorGUILayout.LabelField("Background Colors", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_bgNormal);
        EditorGUILayout.PropertyField(_bgHover);
        EditorGUILayout.PropertyField(_bgPressed);
        
        EditorGUILayout.Space(5);
        
        // === Frame Colors ===
        EditorGUILayout.LabelField("Frame Colors", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_frameNormal);
        EditorGUILayout.PropertyField(_frameHover);
        EditorGUILayout.PropertyField(_framePressed);
        
        EditorGUILayout.Space(5);
        
        // === Animation ===
        EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_fadeDuration);
        
        serializedObject.ApplyModifiedProperties();
    }
}