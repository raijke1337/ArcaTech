// Place in Editor folder
// File: DescriptionEditor.cs

using UnityEditor;
using UnityEngine;

namespace Arcatech.Texts.Editor
{
    [CustomEditor(typeof(Description))]
    public class DescriptionEditor : UnityEditor.Editor
    {
        private GUIStyle _cardStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _labelStyle;

        private void InitStyles()
        {
            if (_cardStyle != null) return;
            
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
            InitStyles();
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
                Texture2D preview = AssetPreview.GetAssetPreview(sprite);
                if (preview != null)
                {
                    GUILayout.Label(preview, GUILayout.Height(120), GUILayout.ExpandWidth(true));
                }
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
        
        // Static method to draw a Description card given a Description object
        public static void DrawDescriptionCard(Description description)
        {
            if (description == null)
            {
                EditorGUILayout.HelpBox("No Description assigned", MessageType.Info);
                return;
            }
            
            GUIStyle cardStyle = new GUIStyle("box")
            {
                padding = new RectOffset(16, 16, 16, 16),
                margin = new RectOffset(0, 0, 10, 10)
            };

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };

            GUIStyle labelStyle = new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold
            };
            
            EditorGUILayout.BeginVertical(cardStyle);
            
            // Title
            if (!string.IsNullOrWhiteSpace(description.Title))
            {
                GUILayout.Label(description.Title, titleStyle);
                EditorGUILayout.Space();
            }
            
            // Picture
            if (description.Picture != null)
            {
                Texture2D preview = AssetPreview.GetAssetPreview(description.Picture);
                if (preview != null)
                {
                    GUILayout.Label(preview, GUILayout.Height(120), GUILayout.ExpandWidth(true));
                }
            }
            
            EditorGUILayout.Space();
            
            // Description text
            if (!string.IsNullOrWhiteSpace(description.Text))
            {
                EditorGUILayout.LabelField("Description", labelStyle);
                EditorGUILayout.LabelField(description.Text, EditorStyles.wordWrappedLabel);
                EditorGUILayout.Space();
            }
            
            // Flavor text
            if (!string.IsNullOrWhiteSpace(description.FlavorText))
            {
                EditorGUILayout.LabelField("Flavor Text", labelStyle);
                GUIStyle italicStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
                {
                    fontStyle = FontStyle.Italic
                };
                EditorGUILayout.LabelField(description.FlavorText, italicStyle);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        // Static method to draw editable Description card from SerializedObject
        public static void DrawEditableDescriptionCard(SerializedObject descriptionSO)
        {
            if (descriptionSO == null) return;
            
            GUIStyle cardStyle = new GUIStyle("box")
            {
                padding = new RectOffset(16, 16, 16, 16),
                margin = new RectOffset(0, 0, 10, 10)
            };

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };

            GUIStyle labelStyle = new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold
            };
            
            descriptionSO.Update();
            
            var titleProp = descriptionSO.FindProperty("Title");
            var textProp = descriptionSO.FindProperty("Text");
            var pictureProp = descriptionSO.FindProperty("Picture");
            var flavorProp = descriptionSO.FindProperty("FlavorText");

            EditorGUILayout.BeginVertical(cardStyle);

            EditorGUILayout.PropertyField(titleProp, new GUIContent("Title"));
            if (!string.IsNullOrWhiteSpace(titleProp.stringValue))
            {
                GUILayout.Label(titleProp.stringValue, titleStyle);
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Picture", labelStyle);
            var sprite = pictureProp.objectReferenceValue as Sprite;
            if (sprite != null)
            {
                Texture2D preview = AssetPreview.GetAssetPreview(sprite);
                if (preview != null)
                {
                    GUILayout.Label(preview, GUILayout.Height(120), GUILayout.ExpandWidth(true));
                }
            }

            EditorGUILayout.PropertyField(pictureProp, GUIContent.none);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Description", labelStyle);
            textProp.stringValue = EditorGUILayout.TextArea(textProp.stringValue, GUILayout.Height(90));

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Flavor Text", labelStyle);
            flavorProp.stringValue = EditorGUILayout.TextArea(flavorProp.stringValue, GUILayout.Height(60));

            EditorGUILayout.EndVertical();

            descriptionSO.ApplyModifiedProperties();
        }
    }
}