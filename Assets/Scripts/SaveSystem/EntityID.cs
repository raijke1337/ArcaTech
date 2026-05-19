using UnityEditor;
using UnityEngine;

namespace Arcatech.SaveSystem
{
    public class EntityID : MonoBehaviour
    {
        
        [SerializeField]
        private string uniqueId = "";

        /// <summary>
        /// Public read‑only accessor for the ID.
        /// </summary>
        public string UniqueId => uniqueId;

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Не выполняем в режиме игры или если ID уже назначен
            if (Application.isPlaying || !string.IsNullOrEmpty(uniqueId))
                return;

            var currentScene = gameObject.scene;

            // 1. Отсекаем объекты, которые просто лежат в окне Project (у них нет сцены)
            if (!currentScene.IsValid())
                return;

            // 2. ОТСЕКАЕМ PREFAB MODE (Именно здесь была проблема!)
            // Если объект открыт в редакторе префабов, его виртуальная сцена имеет расширение .prefab
            if (!string.IsNullOrEmpty(currentScene.path) && currentScene.path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase))
                return;

            // 3. Официальная проверка Unity на всякий случай (например, при импорте или запекании)
            if (PrefabUtility.IsPartOfPrefabAsset(this))
                return;

            // Если скрипт дошел сюда, значит объект точно находится на обычной игровой сцене.
            // Генерируем ID и помечаем сцену как измененную.
            uniqueId = System.Guid.NewGuid().ToString();
            EditorUtility.SetDirty(this);
        }
#endif

        /// <summary>
        /// For runtime‑instantiated objects (e.g. from a pool or Instantiate()):
        /// if no ID was serialised, generate a new one.
        /// </summary>
        private void Awake()
        {
            if (string.IsNullOrEmpty(uniqueId))
            {
                uniqueId = System.Guid.NewGuid().ToString();
            }
        }
    }
}