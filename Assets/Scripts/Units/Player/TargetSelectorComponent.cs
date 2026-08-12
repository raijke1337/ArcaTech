using UnityEngine;

namespace Arcatech.Units.Control
{
    /// <summary>
    /// the component used to assess targets when using gamepad auto-aim
    /// </summary>
    public sealed class TargetSelector : MonoBehaviour
    {
        [SerializeField] private LayerMask targetMask;
        [SerializeField] private float searchRadius = 15f;
        [SerializeField] private float searchAngle = 45f;

        public BaseGameEntityComponent FindTarget(
            Vector3 origin,
            Vector3 direction)
        {
            Collider[] colliders = Physics.OverlapSphere(
                origin,
                searchRadius,
                targetMask);

            BaseGameEntityComponent best = null;
            float bestScore = float.MinValue;

            foreach (Collider collider in colliders)
            {
                if (!collider.TryGetComponent(
                        out BaseGameEntityComponent entity))
                {
                    continue;
                }

                if (!entity.Targetable)
                    continue;

                Vector3 toTarget =
                    entity.EffectSpawn.position - origin;

                toTarget.y = 0f;

                float distance = toTarget.magnitude;

                if (distance < 0.001f)
                    continue;

                Vector3 targetDirection =
                    toTarget / distance;

                float angle = Vector3.Angle(
                    direction,
                    targetDirection);

                if (angle > searchAngle)
                    continue;

                float alignment = Vector3.Dot(
                    direction,
                    targetDirection);

                float score =
                    alignment * 10f -
                    distance;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = entity;
                }
            }

            return best;
        }
    }
}