using Arcatech.EventBus;
using CartoonFX;
using UnityEngine;



namespace Arcatech.Effects
{
    /// <summary>
    /// Запрос на воспроизведение CFXR-эффекта.
    /// Если Parent задан, Position и Rotation интерпретируются как локальные.
    /// Иначе — как мировые.
    /// </summary>
    public readonly struct ParticlesEvent : IEvent
    {
        public CFXR_Effect Prefab { get; }
        public Transform Parent { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }

        /// <summary>
        /// True, если Position и Rotation являются локальными относительно Parent.
        /// </summary>
        public bool IsLocalSpace => Parent != null;

        /// <summary>
        /// Эффект в мировой позиции.
        /// </summary>
        public ParticlesEvent(
            CFXR_Effect prefab,
            Vector3 worldPosition,
            Quaternion rotation)
        {
            Prefab = prefab;
            Parent = null;
            Position = worldPosition;
            Rotation = rotation;
        }

        /// <summary>
        /// Эффект в мировой позиции с поворотом Quaternion.identity.
        /// </summary>
        public ParticlesEvent(CFXR_Effect prefab, Vector3 worldPosition)
            : this(prefab, worldPosition, Quaternion.identity)
        {
        }

        /// <summary>
        /// Эффект, который становится дочерним для parent.
        /// Position и Rotation задаются в локальных координатах parent.
        /// </summary>
        public ParticlesEvent(
            CFXR_Effect prefab,
            Transform parent,
            Vector3 localPosition,
            Quaternion localRotation)
        {
            Prefab = prefab;
            Parent = parent;
            Position = localPosition;
            Rotation = localRotation;
        }

        public ParticlesEvent(
            CFXR_Effect prefab,
            Transform parent)
            : this(prefab, parent, Vector3.zero, Quaternion.identity)
        {
        }
    }
}