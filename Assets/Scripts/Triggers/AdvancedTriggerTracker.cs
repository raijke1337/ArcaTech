using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Triggers
{
    /// <summary>
    /// Расширяет <see cref="TriggerTrackerComponent"/> точным расчётом направления/нормали
    /// удара (с учётом скоростей риджидбоди — своей и объекта, в который попали) и
    /// отладочной визуализацией последних попаданий через гизмо в редакторе.
    /// </summary>
    public class AdvancedTriggerTracker : TriggerTrackerComponent
    {
        // todo: maybe move into a game rule config
        private const float ImpactDirectionEpsilon = 1e-6f;
        private const float RayOriginOffset = 0.05f;
        private const float RaycastRange = RayOriginOffset * 3f;

        // updated for better precision
        protected override (Vector3 position, Vector3 direction, Vector3 normal) CalculateHitGeometry(Collider other)
        {
            if (other == null)
                return (transform.position, Vector3.zero, Vector3.zero);

            var rb = other.attachedRigidbody;
            var hitPosition = transform.position; // other.bounds.center; //other.ClosestPoint(transform.position);
            var rawDirection = hitPosition - transform.position;
            var impactDirection = ResolveImpactDirection(rawDirection, rb);
            var hitNormal = -impactDirection;

            return (hitPosition, impactDirection, hitNormal);
        }

        private Vector3 ResolveImpactDirection(Vector3 candidate, Rigidbody otherRigidbody = null)
        {
            // hit on a moving enemy or a platform with a rigidbody
            if (otherRigidbody != null)
            {
                // 1. Try 'other' object's velocity if trigger doesn't move much

                if (otherRigidbody.linearVelocity.sqrMagnitude > ImpactDirectionEpsilon &&
                    CachedRigidbody.linearVelocity.sqrMagnitude <= ImpactDirectionEpsilon)
                {
                    return otherRigidbody.linearVelocity.normalized;
                }
                // 2. Try relative velocity, for example, an enemy hit

                Vector3 relativeVelocity = otherRigidbody.linearVelocity - CachedRigidbody.linearVelocity;
                if (relativeVelocity.sqrMagnitude > ImpactDirectionEpsilon)
                {
                    return relativeVelocity.normalized;
                }
            }
            // no rigidbody, so a wall, most likely
            else
            {
                // 3. Try trigger's velocity if it has one
                if (CachedRigidbody.linearVelocity.sqrMagnitude > ImpactDirectionEpsilon)
                {
                    return CachedRigidbody.linearVelocity.normalized;
                }
            }

            // 4. Fallback to candidate (from closest point) IF it's not based on *center* to point
            if (candidate.sqrMagnitude > ImpactDirectionEpsilon)
            {
                // This one is used for hits on enemies
                //  Debug.LogWarning($"Fallback to relative velocity of {candidate}");
                return -candidate.normalized;
            }

            // 5. Fallback to object's forward direction
            if (transform.forward.sqrMagnitude > ImpactDirectionEpsilon)
            {
                //Debug.LogWarning($"Fallback to {this.name} forward direction");
                return transform.forward.normalized;
            }

            Debug.LogWarning("Failed to calculate impact direction, returning Vector3.forward on " + gameObject.name);
            return Vector3.forward;
        }

#if UNITY_EDITOR
        // Configuration for debug visualization (adjust as needed)
        [SerializeField, Tooltip("Duration (seconds) to show hit visualizations")]
        private float _hitVisualizationDuration = 2f;

        [SerializeField, Tooltip("Radius of wire sphere for hit positions")]
        private float _hitSphereRadius = 0.1f;

        [SerializeField, Tooltip("Length of lines for normal/direction")]
        private float _lineLength = 0.5f;

        // Tracks recent hits (position, direction, normal, timestamp). Limited to last 10 for performance.
        private readonly List<(Vector3 position, Vector3 direction, Vector3 normal, float timestamp)> _recentHits =
            new();

        protected override void OnHitRegistered(Vector3 position, Vector3 direction, Vector3 normal)
        {
            AddHitForVisualization(position, direction, normal);
        }

        private void OnDrawGizmos()
        {
            // Draw trigger bounds first
            Gizmos.color = ReceiverCount == 0 ? Color.black : Color.blue;
            if (Active) Gizmos.color = Color.white;
            DrawTriggerBounds();

            // Draw recent hit visualizations
            var currentTime = Time.time;
            int i = 0;
            while (i < _recentHits.Count)
            {
                var (pos, dir, norm, time) = _recentHits[i];
                float age = currentTime - time;
                if (age > _hitVisualizationDuration)
                {
                    // Remove expired hits
                    _recentHits.RemoveAt(i);
                    continue;
                }

                // Draw wire sphere at hit position (semi-transparent based on age for fade effect)
                Gizmos.color = Color.Lerp(Color.green, Color.clear, age / _hitVisualizationDuration);
                Gizmos.DrawWireSphere(pos, _hitSphereRadius);

                // Draw line for impact direction (e.g., red)
                Gizmos.color = Color.red;
                Gizmos.DrawLine(pos, pos + dir * _lineLength);

                // Draw line for hit normal (e.g., blue)
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(pos, pos + norm * _lineLength);

                i++;
            }
        }

        private void DrawTriggerBounds()
        {
            if (TriggerCollider == null) return;

            // Generalized drawing for common collider types
            if (TriggerCollider is BoxCollider box)
            {
                Gizmos.matrix = Matrix4x4.TRS(
                    box.transform.TransformPoint(box.center),
                    box.transform.rotation,
                    Vector3.Scale(box.transform.lossyScale, box.size)
                );
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            }
            else if (TriggerCollider is SphereCollider sphere)
            {
                var scale = Mathf.Max(sphere.transform.lossyScale.x,
                    Mathf.Max(sphere.transform.lossyScale.y,
                        sphere.transform.lossyScale.z)); // Fix: Manual max component
                var center = sphere.transform.TransformPoint(sphere.center);
                Gizmos.DrawWireSphere(center, sphere.radius * scale);
            }
            else if (TriggerCollider is CapsuleCollider capsule)
            {
                var maxScale = Mathf.Max(capsule.transform.lossyScale.x,
                    Mathf.Max(capsule.transform.lossyScale.y,
                        capsule.transform.lossyScale.z)); // Fix: Manual max component
                var center = capsule.transform.TransformPoint(capsule.center);
                var adjustedHeight =
                    Mathf.Max(0f, capsule.height * maxScale - capsule.radius * 2f * maxScale) /
                    2f; // Ensure non-negative and halve for offset
                var radialDirection = capsule.direction == 0 ? Vector3.right :
                    capsule.direction == 1 ? Vector3.up : Vector3.forward;
                var startPos = center - radialDirection * adjustedHeight;
                var end = center + radialDirection * adjustedHeight;

                Gizmos.DrawWireSphere(startPos, capsule.radius * maxScale);
                Gizmos.DrawWireSphere(end, capsule.radius * maxScale);
                Gizmos.DrawLine(startPos, end);
            }
            else
            {
                // Fallback for MeshColliders or unknowns: Draw bounds as a wire box
                Gizmos.DrawWireCube(TriggerCollider.bounds.center, TriggerCollider.bounds.size);
            }

            Gizmos.matrix = Matrix4x4.identity; // Reset matrix
        }

        // Helper to add a hit for visualization (called from OnHitRegistered)
        private void AddHitForVisualization(Vector3 position, Vector3 direction, Vector3 normal)
        {
            if (_recentHits.Count >= 10) _recentHits.RemoveAt(0); // Cap size
            _recentHits.Add((position, direction, normal, Time.time));
        }
#endif
    }
}