using UnityEditor;
using UnityEngine;
namespace Arcatech
{
    [CustomPropertyDrawer(typeof(ReadOnlyTextAttribute))]
    public class ReadOnlyTextDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Сохраняем предыдущее состояние активности GUI
            bool wasEnabled = GUI.enabled;

            // Выключаем возможность редактирования
            GUI.enabled = false;

            // Рисуем само поле
            EditorGUI.PropertyField(position, property, label);

            // Восстанавливаем прежнее состояние активности GUI
            GUI.enabled = wasEnabled;
        }
    }
        
        
        
}