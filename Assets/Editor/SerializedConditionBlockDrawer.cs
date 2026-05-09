using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Arcatech.Units
{
    [CustomPropertyDrawer(typeof(SerializedConditionBlock))]
    public class SerializedConditionBlockDrawer : PropertyDrawer
    {
        private const float LineHeight = 18f;
        private const float VerticalSpacing = 2f;
        private const float ToggleWidth = 18f;

        private static readonly Dictionary<string, bool> FoldoutState = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool expanded = GetFoldout(property);
            Rect foldoutRect = new Rect(position.x, position.y, position.width, LineHeight);
            expanded = EditorGUI.Foldout(foldoutRect, expanded, label, true);
            SetFoldout(property, expanded);

            if (!expanded) return;

            EditorGUI.indentLevel++;
            float y = foldoutRect.yMax + VerticalSpacing;

            SerializedProperty operatorProp = property.FindPropertyRelative("Operator");
            SerializedProperty negateProp = property.FindPropertyRelative("NegateResult");
            SerializedProperty conditionsProp = property.FindPropertyRelative("Conditions");
            SerializedProperty nestedProp = property.FindPropertyRelative("NestedBlocks");

            Rect operatorRect = new Rect(position.x, y, position.width - ToggleWidth, LineHeight);
            Rect negateRect = new Rect(operatorRect.xMax + 2f, y, ToggleWidth, LineHeight);

            EditorGUI.PropertyField(operatorRect, operatorProp);
            negateProp.boolValue = EditorGUI.ToggleLeft(negateRect, "¬", negateProp.boolValue);
            y += LineHeight + VerticalSpacing;

            Rect conditionsRect = new Rect(position.x, y, position.width, EditorGUI.GetPropertyHeight(conditionsProp, true));
            EditorGUI.PropertyField(conditionsRect, conditionsProp, true);
            y = conditionsRect.yMax + VerticalSpacing;

            Rect nestedRect = new Rect(position.x, y, position.width, EditorGUI.GetPropertyHeight(nestedProp, true));
            EditorGUI.PropertyField(nestedRect, nestedProp, true);

            EditorGUI.indentLevel--;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = LineHeight;
            if (!GetFoldout(property)) return height;

            SerializedProperty conditionsProp = property.FindPropertyRelative("Conditions");
            SerializedProperty nestedProp = property.FindPropertyRelative("NestedBlocks");

            height += VerticalSpacing + LineHeight; // Operator + negate row
            height += VerticalSpacing + EditorGUI.GetPropertyHeight(conditionsProp, true);
            height += VerticalSpacing + EditorGUI.GetPropertyHeight(nestedProp, true);

            return height;
        }

        private static bool GetFoldout(SerializedProperty property)
        {
            string key = property.propertyPath;
            if (!FoldoutState.TryGetValue(key, out bool state))
            {
                state = true;
                FoldoutState[key] = state;
            }
            return state;
        }

        private static void SetFoldout(SerializedProperty property, bool state)
        {
            FoldoutState[property.propertyPath] = state;
        }
    }
}