using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(LevelConditionsManager.VerifiedUnitCondition))]
public class VerifiedUnitConditionDrawer : PropertyDrawer
{
    private const float SPACING = 4f;
    private const float LABEL_WIDTH = 50f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Draw the main foldout label
        var foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            // Calculate field positions
            var fieldY = position.y + EditorGUIUtility.singleLineHeight + 2;
            var fieldWidth = (position.width - SPACING) * 0.5f;
            
            var targetRect = new Rect(position.x, fieldY, fieldWidth, EditorGUIUtility.singleLineHeight);
            var itemRect = new Rect(position.x + fieldWidth + SPACING, fieldY, fieldWidth, EditorGUIUtility.singleLineHeight);
            var checkRect = new Rect(position.x, fieldY + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);

            // Get properties
            var targetProp = property.FindPropertyRelative("target");
            var itemProp = property.FindPropertyRelative("item");
            var checkOnlyProp = property.FindPropertyRelative("checkOnlyOnQuery");

            // Draw fields with custom labels
            EditorGUI.PropertyField(targetRect, targetProp, new GUIContent("Target Unit"));
            EditorGUI.PropertyField(itemRect, itemProp, new GUIContent("Interactive Item"));
            EditorGUI.PropertyField(checkRect, checkOnlyProp, new GUIContent("Check Only On Query"));

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.isExpanded)
        {
            return EditorGUIUtility.singleLineHeight * 3 + 6; // 3 lines + spacing
        }
        return EditorGUIUtility.singleLineHeight; // Just the foldout
    }
}
