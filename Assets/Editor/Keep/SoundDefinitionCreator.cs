using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace Arcatech.Audio.Editor
{
    /// <summary>
    /// Позволяет создавать SoundDefinition сразу с выбранным AudioClip
    /// или AudioRandomContainer через контекстное меню в Project-окне.
    /// </summary>
    public static class SoundDefinitionCreator
    {
        private const string MenuPath = "Assets/Create/Audio/Sound Definition From Selection";

        [MenuItem(MenuPath, true)]
        private static bool Validate()
        {
            // Пункт меню активен, только если среди выделенного
            // есть хотя бы один AudioResource (AudioClip или AudioRandomContainer)
            foreach (var obj in Selection.objects)
            {
                if (obj is AudioResource)
                    return true;
            }
            return false;
        }

        [MenuItem(MenuPath, false, 10)]
        private static void CreateFromSelection()
        {
            Object[] selection = Selection.objects;
            Object lastCreated = null;

            foreach (var obj in selection)
            {
                if (obj is AudioResource resource)
                {
                    lastCreated = CreateSoundDefinitionAsset(resource);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (lastCreated != null)
            {
                EditorGUIUtility.PingObject(lastCreated);
                Selection.activeObject = lastCreated;
            }
        }

        private static Object CreateSoundDefinitionAsset(AudioResource resource)
        {
            string sourcePath = AssetDatabase.GetAssetPath(resource);
            string sourceFolder = Path.GetDirectoryName(sourcePath)?.Replace("\\", "/");
            if (string.IsNullOrEmpty(sourceFolder))
                sourceFolder = "Assets";

            string cleanName = resource.name;
            string assetName = $"SO_Sound_{cleanName}.asset";
            string fullPath = AssetDatabase.GenerateUniqueAssetPath($"{sourceFolder}/{assetName}");

            var definition = ScriptableObject.CreateInstance<SoundDefinition>();
            definition.resource = resource;

            AssetDatabase.CreateAsset(definition, fullPath);

            return definition;
        }
    }
}