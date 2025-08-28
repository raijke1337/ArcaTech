using Unity.AppUI.UI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
namespace Arcatech.Stats
{
    [CustomPropertyDrawer(typeof(StatValueContainer))]
    public class StatValueContainerDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();
            var min = new PropertyField(property.FindPropertyRelative("_minValue"));
            var max = new PropertyField(property.FindPropertyRelative("_maxValue"));
            var cur = new PropertyField(property.FindPropertyRelative("_currentValue"));

            container.Add(min);
            container.Add(new Text(" / "));
            container.Add(cur);
            container.Add(new Text(" / "));
            container.Add(max);

            return container;
        }
    }
}