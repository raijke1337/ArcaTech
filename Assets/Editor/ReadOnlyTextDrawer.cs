using UnityEditor;
using UnityEngine;
namespace Arcatech
{
    [CustomPropertyDrawer(typeof(ReadOnlyTextAttribute))]
    public class ReadOnlyTextDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool wasEnabled = GUI.enabled;
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = wasEnabled;
        }
    }
}