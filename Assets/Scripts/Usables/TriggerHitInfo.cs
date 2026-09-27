using System;
using Arcatech.Triggers;
using UnityEngine;

namespace Arcatech
{
    [Serializable]
    public struct TriggerHitInfo
    {
        public TriggerHitInfo(ITriggerNotificationProvider triggerNotificationProvider,
            Collider hit,
            Vector3 hitPosition, Vector3 impactDirection, Vector3 hitNormal,
            HitLayerKind layerKind,
            float time)
        {
            Source = triggerNotificationProvider;
            TargetCollider = hit;
            Position = hitPosition;
            ImpactDirection = impactDirection; // From the incoming projectile
            Normal = hitNormal; // Normal of the surface hit
            LayerKind = layerKind;
            Time = time;
            TargetCollider.TryGetComponent(out _targetEntity);
        }

        private BaseGameEntityComponent _targetEntity;

        /// <summary>
        /// helper method to avoid endless TryGetComponent()s
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>hit on entity</returns>
        public bool TryGetEntityTarget(out BaseGameEntityComponent entity)
        {
            entity = _targetEntity;
            return entity != null;
        }
        public ITriggerNotificationProvider Source { get; }

        /// <summary>
        /// The hit collider.
        /// </summary>
        public Collider TargetCollider { get; }

        /// <summary>
        /// The  point in world space where the hit occurred.
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        /// The direction of the incoming projectile at the moment of impact.
        /// </summary>
        public Vector3 ImpactDirection { get; }

        /// <summary>
        /// The normal vector of the surface that was hit.
        /// </summary>
        public Vector3 Normal { get; }

        /// <summary>
        /// На каком из настроенных слоёв (Valid/Invalid) произошло попадание.
        /// Позволяет получателям отличать "полезные" хиты от "невалидных" без
        /// повторной проверки маски слоя коллайдера.
        /// </summary>
        public HitLayerKind LayerKind { get; }

        /// <summary>
        /// The Unity Time.time when the hit occurred.
        /// </summary>
        public float Time { get; }
    }
    /// <summary>
    /// К какому из настроенных слоёв (см. DataManager.GameRules.ValidHitsLayer /
    /// InvalidHitsLayer) относится объект, по которому было зарегистрировано попадание.
    /// Кладётся в <see cref="Arcatech.TriggerHitInfo"/>, чтобы получателям не приходилось
    /// заново проверять слой коллайдера.
    /// </summary>
    public enum HitLayerKind
    {
        /// <summary>
        /// Коллайдер не входит ни в ValidHitsLayer, ни в InvalidHitsLayer.
        /// В обычном физическом OnTriggerEnter такого не бывает (включённые слои
        /// триггера уже это отфильтровывают), но возможно, например, если маски
        /// не сконфигурированы.
        /// </summary>
        Unknown = 0,
        Valid,
        Invalid
    }
}