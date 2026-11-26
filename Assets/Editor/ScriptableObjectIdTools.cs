using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Arcatech
{
    internal static class ScriptableObjectIdTools
    {
        private const string ContextMenu = "Assets/Regenerate ScriptableObject GUID";
        private const string RepairMenu = "Arcatech/Tools/Fix Duplicate ScriptableObject GUIDs";

        [MenuItem(ContextMenu, true)]
        private static bool CanRegenerateSelection()
        {
            return Selection.objects.Any(asset => asset is ScriptableObjectID && EditorUtility.IsPersistent(asset));
        }

        [MenuItem(ContextMenu)]
        private static void RegenerateSelection()
        {
            var assets = Selection.objects.OfType<ScriptableObjectID>()
                .Where(EditorUtility.IsPersistent)
                .ToArray();
            RegenerateIds(assets, "Regenerate ScriptableObject GUIDs");
        }

        [MenuItem(RepairMenu)]
        private static void FixDuplicates()
        {
            var assets = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .SelectMany(AssetDatabase.LoadAllAssetsAtPath)
                .OfType<ScriptableObjectID>()
                .ToArray();

            var duplicates = assets.Where(asset => !string.IsNullOrEmpty(asset.ID))
                .GroupBy(asset => asset.ID, StringComparer.Ordinal)
                .SelectMany(group => group.OrderBy(AssetDatabase.GetAssetPath, StringComparer.Ordinal)
                    .ThenBy(GetLocalFileId)
                    .Skip(1))
                .ToArray();

            RegenerateIds(duplicates, "Fix Duplicate ScriptableObject GUIDs");
            Debug.Log($"Checked {assets.Length} ScriptableObject IDs. Regenerated {duplicates.Length} duplicates.");
        }

        private static long GetLocalFileId(ScriptableObjectID asset)
        {
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string _, out long localFileId);
            return localFileId;
        }

        internal static void RegenerateIds(ScriptableObjectID[] assets, string undoName)
        {
            if (assets.Length == 0) return;

            Undo.RegisterCompleteObjectUndo(assets, undoName);
            foreach (var asset in assets)
            {
                var oldId = asset.ID;
                asset.ID = Guid.NewGuid().ToString();
                EditorUtility.SetDirty(asset);
                Debug.Log($"Regenerated GUID for '{asset.name}' ({AssetDatabase.GetAssetPath(asset)}): {oldId} -> {asset.ID}", asset);
            }

            ItemIdIndex.Invalidate();
            foreach (var asset in assets)
                AssetDatabase.SaveAssetIfDirty(asset);
        }
    }
}
