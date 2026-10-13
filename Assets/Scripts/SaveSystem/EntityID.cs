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

        public string UniqueId
        {
            get { EnsureId(); return uniqueId; }
        }

        /// <summary>true - объект создан во время игры (Instantiate префаба: спавнеры,
        /// призыв, волны). Такие объекты не сохраняются в прогрессе уровня: при откате
        /// к чекпоинту они пропадают и могут заспавниться заново.</summary>
        public bool IsRuntimeSpawned
        {
            get { EnsureId(); return _runtimeSpawned; }
        }

        private bool _runtimeSpawned;

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

        /// <summary>Для объектов, скопированных через Ctrl+D: ID копируется вместе с объектом,
        /// и две сущности начинают делить одно состояние. Сбросить и выдать новый.</summary>
        [ContextMenu("Regenerate ID")]
        private void RegenerateId()
        {
            Undo.RecordObject(this, "Regenerate Entity ID");
            uniqueId = System.Guid.NewGuid().ToString();
            EditorUtility.SetDirty(this);
        }
#endif

        private void Awake() => EnsureId();

        // Ленивая генерация: другие компоненты могут спросить ID в своём OnEnable
        // раньше, чем у этого компонента вызовется Awake.
        private void EnsureId()
        {
            if (!string.IsNullOrEmpty(uniqueId) || !Application.isPlaying) return;
            uniqueId = System.Guid.NewGuid().ToString();
            _runtimeSpawned = true;
        }
    }
}
