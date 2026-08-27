using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Arcatech.SaveSystem
{
    public class EntityID : MonoBehaviour
    {
        [SerializeField]
        private string uniqueId = "";

        public string UniqueId => uniqueId;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying || !string.IsNullOrEmpty(uniqueId))
                return;

            var currentScene = gameObject.scene;

            if (!currentScene.IsValid())
                return;

            if (!string.IsNullOrEmpty(currentScene.path) &&
                currentScene.path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase))
                return;

            if (PrefabUtility.IsPartOfPrefabAsset(this))
                return;

            uniqueId = System.Guid.NewGuid().ToString();
            EditorUtility.SetDirty(this);
        }
#endif

        private void Awake()
        {
            if (string.IsNullOrEmpty(uniqueId))
            {
                uniqueId = System.Guid.NewGuid().ToString();
            }
        }
    }
}