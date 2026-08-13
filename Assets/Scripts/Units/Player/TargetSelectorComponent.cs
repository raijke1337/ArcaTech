using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Units.Control
{
    /// <summary>
    /// Finds the closest valid target inside an aiming cone.
    /// </summary>
    public sealed class TargetSelector : MonoBehaviour
    {
        public BaseGameEntityComponent FindClosestTarget(
            Vector3 origin,
            Vector3 direction,
            Transform ignoredRoot,
            LayerMask targetMask,
            float searchRadius,
            float searchAngle)
        {
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.0001f)
                return null;

            direction.Normalize();

            Collider[] colliders = Physics.OverlapSphere(
                origin,
                searchRadius,
                targetMask,
                QueryTriggerInteraction.Ignore);

            BaseGameEntityComponent closestTarget = null;
            float closestDistanceSqr = float.MaxValue;

            var checkedEntities = new HashSet<BaseGameEntityComponent>();

            foreach (Collider collider in colliders)
            {
                if (collider == null)
                    continue;

                BaseGameEntityComponent entity =
                    collider.GetComponentInParent<BaseGameEntityComponent>();

                if (entity == null)
                    continue;

                if (!checkedEntities.Add(entity))
                    continue;

                if (!IsValidCandidate(entity, ignoredRoot))
                    continue;

                Vector3 toTarget = entity.EffectSpawn.position - origin;
                toTarget.y = 0f;

                float distanceSqr = toTarget.sqrMagnitude;

                if (distanceSqr < 0.0001f)
                    continue;

                Vector3 targetDirection = toTarget.normalized;

                float angle = Vector3.Angle(direction, targetDirection);

                if (angle > searchAngle)
                    continue;

                if (distanceSqr >= closestDistanceSqr)
                    continue;

                closestDistanceSqr = distanceSqr;
                closestTarget = entity;
            }

            return closestTarget;
        }

        private static bool IsValidCandidate(
            BaseGameEntityComponent entity,
            Transform ignoredRoot)
        {
            if (entity == null ||
                entity.EffectSpawn == null ||
                !entity.gameObject.activeInHierarchy ||
                !entity.Targetable)
            {
                return false;
            }

            // Не выбираем самого игрока и его дочерние объекты.
            if (ignoredRoot != null &&
                entity.transform.IsChildOf(ignoredRoot))
            {
                return false;
            }

            // Дополнительная защита: Player никогда не является целью.
            if (entity.CompareTag("Player"))
                return false;

            return true;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 15f);
        }
#endif
    }
}