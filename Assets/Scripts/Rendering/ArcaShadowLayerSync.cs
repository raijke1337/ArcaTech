using UnityEngine;

namespace Arcatech.Rendering
{
    /// <summary>
    /// Переносит GameObject-слой в Rendering Layer, чтобы лампы с
    /// Custom Shadow Layers могли исключить объект из своих теней.
    ///
    /// Зачем: маска теней у Light работает только с Rendering Layers, а не с
    /// обычными слоями (Entities и т. п.). Компонент вешается на корень префаба
    /// персонажа или врага; всем дочерним рендерерам на нужном GameObject-слое
    /// он выставляет Rendering Layer Mask.
    ///
    /// Оружие и экипировка, добавленные в рантайме: вызовите <see cref="Apply"/>
    /// после экипировки (прямые дочерние объекты подхватываются автоматически).
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class ArcaShadowLayerSync : MonoBehaviour
    {
        [Tooltip("Переводить все MeshRenderer/SkinnedMeshRenderer в иерархии, не глядя на GameObject-слой. " +
                 "Нужно, когда модель и костюм спавнятся в рантайме на слое Default.")]
        [SerializeField] private bool ignoreGameObjectLayer = true;

        [Tooltip("GameObject-слои, чьи рендереры нужно перевести (если ignoreGameObjectLayer выключен).")]
        [SerializeField] private LayerMask sourceLayers;

        [Tooltip("Rendering Layer, который получат рендереры (например, Entities). " +
                 "Default снимается, иначе лампа всё равно будет их учитывать.")]
        [SerializeField] private RenderingLayerMask renderingLayer;

        private void Reset()
        {
            sourceLayers = LayerMask.GetMask("Entities");
        }

        private int _lastHierarchyCount = -1;

        private void Awake() => Apply();
        private void OnValidate() => Apply();
        private void OnTransformChildrenChanged() => Apply();

        // Костюмы и оружие спавнятся в рантайме и не всегда прямыми детьми корня
        // (OnTransformChildrenChanged их не видит), поэтому следим за размером иерархии.
        private void LateUpdate()
        {
            if (!Application.isPlaying) return;
            int count = transform.hierarchyCount;
            if (count == _lastHierarchyCount) return;
            _lastHierarchyCount = count;
            Apply();
        }

        /// <summary>Применить ко всем дочерним рендерерам (включая неактивные).</summary>
        public void Apply()
        {
            if ((uint)renderingLayer == 0u) return;

            foreach (var r in GetComponentsInChildren<Renderer>(true))
            {
                if (!(r is MeshRenderer || r is SkinnedMeshRenderer)) continue;
                if (!ignoreGameObjectLayer && (sourceLayers.value & (1 << r.gameObject.layer)) == 0) continue;
                r.renderingLayerMask = renderingLayer;
            }
        }
    }
}
