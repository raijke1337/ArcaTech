using System;
using System.Collections.Generic;
using Arcatech.Managers;
using Arcatech.Units;
using DG.Tweening;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Arcatech.Triggers
{
    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    public class TriggerTrackerComponent : ValidatedMonoBehaviour, ITriggerNotificationProvider
    {
        #region ITriggerNotificationProvider

        private readonly HashSet<ITriggerNotificationReceiver> _receivers = new();
        public bool Active { get; set; } = true;

        public void RegisterReceiver(ITriggerNotificationReceiver r)
        {
            _receivers.Add(r);
        }

        public void UnregisterReceiver(ITriggerNotificationReceiver r) => _receivers.Remove(r);

        #endregion

        [SerializeField, Self] private Collider triggerCollider;
        [SerializeField, Self] private Rigidbody cachedRigidbody;

        private LayerMask _valid;
        private LayerMask _invalid;


        // todo: maybe move into a game rule config

        private const float ImpactDirectionEpsilon = 1e-6f;
        private const float RayOriginOffset = 0.05f;
        private const float RaycastRange = RayOriginOffset * 3f;



        private void Start()
        {
            triggerCollider.isTrigger = true;
            cachedRigidbody.isKinematic = true;

            _valid = LayerMask.GetMask(DataManager.GameRules.ValidHitsLayer);
            _invalid = LayerMask.GetMask(DataManager.GameRules.InvalidHitsLayer);

            triggerCollider.includeLayers = (_valid | _invalid);

            var r = GetComponentsInChildren<ITriggerNotificationReceiver>();
            foreach (var r2 in r) RegisterReceiver(r2);
            if (_receivers.Count == 0) Active = false;
        }

        private bool CanNotify() =>
            Active && _receivers.Count > 0;


        public void AreaCast(ITriggerNotificationReceiver receiver)
        {
            if (triggerCollider is not BoxCollider boxCollider)
            {
                Debug.LogError($"{nameof(AreaCast)} requires a {nameof(BoxCollider)}.", this);
                return;
            }

            Transform boxTransform = boxCollider.transform;

            Vector3 worldCenter = boxTransform.TransformPoint(boxCollider.center);

            Vector3 halfExtents = Vector3.Scale(
                boxCollider.size * 0.5f,
                boxTransform.lossyScale);

            Collider[] found = Physics.OverlapBox(
                worldCenter,
                halfExtents,
                boxTransform.rotation,
                Physics.AllLayers,
                QueryTriggerInteraction.Collide);

            foreach (Collider foundCollider in found)
            {
                if (foundCollider == triggerCollider)
                {
                    continue;
                }

                OnTriggerEnter(foundCollider);
            }
        }

        protected void OnTriggerEnter(Collider other)
        {
            if (!CanNotify() || other.isTrigger) return;

            var hitGeometry = CalculateHitGeometry(other);

            // Создаём копию, чтобы избежать изменений во время итерации
            var receiversCopy = new List<ITriggerNotificationReceiver>(_receivers);

            foreach (var receiver in receiversCopy)
            {
                receiver.TriggerEntered(new TriggerHitInfo(
                    this,
                    other,
                    hitGeometry.position,
                    hitGeometry.direction,
                    hitGeometry.normal,
                    Time.time));
            }
            AddHitForVisualization(hitGeometry.position, hitGeometry.direction, hitGeometry.normal);
        }

        protected void OnTriggerExit(Collider other)
        {
            if (!CanNotify() || other.isTrigger) return;
            foreach (var receiver in _receivers)
            {
                receiver.TriggerExited(new TriggerHitInfo(
                    this,
                    other,
                    other.transform.position,
                    Vector3.up,
                    Vector3.up,
                    Time.time));
            }
        }

        private void OnDisable()
        {
            //_receivers.Clear();
            _attackWarningTween?.Kill();
            if (_attackWarningObject != null) _attackWarningObject.SetActive(false);
        }


        // Simplified: Assume impactDirection provides a reasonable normal for triggers
        private (Vector3 position, Vector3 direction, Vector3 normal) CalculateHitGeometry(Collider other)
        {
            if (other == null)
                return (transform.position, Vector3.zero, Vector3.zero);

            var rb = other.attachedRigidbody;
            var hitPosition = gameObject.transform.position; // other.bounds.center; //other.ClosestPoint(transform.position);
            var rawDirection = hitPosition - transform.position;
            var impactDirection = ResolveImpactDirection(rawDirection, rb);
            var hitNormal = -impactDirection;

            return (hitPosition, impactDirection, hitNormal);
        }

        // updated for better precision
        private Vector3 ResolveImpactDirection(Vector3 candidate, Rigidbody otherRigidbody = null)
        {

            // hit on a moving enemy or a platform with a rigidbody
            if (otherRigidbody != null)
            {
                // 1. Try 'other' object's velocity if trigger doesn't move much 

                if (otherRigidbody.linearVelocity.sqrMagnitude > ImpactDirectionEpsilon &&
                    cachedRigidbody.linearVelocity.sqrMagnitude <= ImpactDirectionEpsilon)
                {
                    return otherRigidbody.linearVelocity.normalized;
                }
                // 2. Try relative velocity, for example, an enemy hit

                Vector3 relativeVelocity = otherRigidbody.linearVelocity - cachedRigidbody.linearVelocity;
                if (relativeVelocity.sqrMagnitude > ImpactDirectionEpsilon)
                {
                    return relativeVelocity.normalized;
                }
            }
            // no rigidbody, so a wall, most likely
            else
            {
                // 3. Try trigger's velocity if it has one
                if (cachedRigidbody.linearVelocity.sqrMagnitude > ImpactDirectionEpsilon)
                {
                    return cachedRigidbody.linearVelocity.normalized;
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

        private void OnDrawGizmos()
        {
            // Draw trigger bounds first
            Gizmos.color = _receivers.Count == 0 ? Color.black : Color.blue;
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
            if (triggerCollider == null) return;

            // Generalized drawing for common collider types
            if (triggerCollider is BoxCollider box)
            {
                Gizmos.matrix = Matrix4x4.TRS(
                    box.transform.TransformPoint(box.center),
                    box.transform.rotation,
                    Vector3.Scale(box.transform.lossyScale, box.size)
                );
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            }
            else if (triggerCollider is SphereCollider sphere)
            {
                var scale = Mathf.Max(sphere.transform.lossyScale.x,
                    Mathf.Max(sphere.transform.lossyScale.y,
                        sphere.transform.lossyScale.z)); // Fix: Manual max component
                var center = sphere.transform.TransformPoint(sphere.center);
                Gizmos.DrawWireSphere(center, sphere.radius * scale);
            }
            else if (triggerCollider is CapsuleCollider capsule)
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
                Gizmos.DrawWireCube(triggerCollider.bounds.center, triggerCollider.bounds.size);
            }

            Gizmos.matrix = Matrix4x4.identity; // Reset matrix
        }

// Helper to add a hit for visualization (call in OnTriggerEnter/OnTriggerExit if on valid/invalid layers)
        private void AddHitForVisualization(Vector3 position, Vector3 direction, Vector3 normal)
        {
            if (_recentHits.Count >= 10) _recentHits.RemoveAt(0); // Cap size
            _recentHits.Add((position, direction, normal, Time.time));
        }
#endif

        public void OnChangeUsableState(StateMachineNotifyType notification)
        {
            switch (notification)
            {
                case StateMachineNotifyType.NoNotify:
                    break;
                case StateMachineNotifyType.Starting:
                    // show a red quad inside the lower bounds area (danger zone warning)
                    ShowAttackWarning();
                    break;
                case StateMachineNotifyType.Use:
                    // hide the quad
                    HideAttackWarning();
                    break;
                case StateMachineNotifyType.EndUse:
                    HideAttackWarning();
                    break;
                case StateMachineNotifyType.Cancel:
                    HideAttackWarning();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(notification), notification, null);
            }
        }

        #region warning

        private GameObject _attackWarningObject;
        private LineRenderer _attackWarningLine;
        private Material _attackWarningMaterial;
        private Tween _attackWarningTween;
        private float _warningAlpha;

        private static readonly int ColorID = Shader.PropertyToID("_Color");
        private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
        
        [SerializeField, Tooltip("Толщина линии контура")]
        private float _warningLineWidth = 0.1f;

        [SerializeField, Tooltip("Смещение контура над полом (избегаем z-fighting)")]
        private float _warningFloorOffset = 0.02f;

        [SerializeField, Tooltip("Длительность резкого fade-in, сек")]
        private float _warningFadeInDuration = 0.12f;

        [SerializeField, Tooltip("Длительность fade-out, сек")]
        private float _warningFadeOutDuration = 0.25f;

        
        private void EnsureAttackWarningVisual()
        {
            if (_attackWarningObject != null) return;

            _attackWarningObject = new GameObject("AttackWarningVisual");
            _attackWarningObject.transform.SetParent(null);

            _attackWarningLine = _attackWarningObject.AddComponent<LineRenderer>();
            _attackWarningLine.useWorldSpace = true;
            _attackWarningLine.loop = true; // замыкаем контур в прямоугольник
            _attackWarningLine.positionCount = 4;
            _attackWarningLine.widthMultiplier = _warningLineWidth;
            _attackWarningLine.numCornerVertices = 2;
            _attackWarningLine.numCapVertices = 2;
            _attackWarningLine.shadowCastingMode = ShadowCastingMode.Off;
            _attackWarningLine.receiveShadows = false;

            // Sprites/Default нативно поддерживает startColor/endColor через vertex color —
            // удобно для fade без доп. манипуляций с материалом.
            var shader = Shader.Find("Sprites/Default")
                         ?? Shader.Find("Universal Render Pipeline/Unlit")
                         ?? Shader.Find("Unlit/Transparent")
                         ?? Shader.Find("Standard");

            _attackWarningMaterial = new Material(shader) { name = "AttackWarningMaterial_Instance" };
            _attackWarningLine.sharedMaterial = _attackWarningMaterial;

            ApplyWarningColor(new Color(1f, 0f, 0f, 0f));
            _attackWarningObject.SetActive(false);
        }
        private void ApplyWarningColor(Color color)
        {
            if (_attackWarningLine != null)
            {
                _attackWarningLine.startColor = color;
                _attackWarningLine.endColor = color;
            }

            // На случай, если шейдер не использует vertex color (URP Unlit и т.п.)
            if (_attackWarningMaterial != null)
            {
                if (_attackWarningMaterial.HasProperty(BaseColorID))
                    _attackWarningMaterial.SetColor(BaseColorID, color);
                if (_attackWarningMaterial.HasProperty(ColorID))
                    _attackWarningMaterial.SetColor(ColorID, color);
            }
        }
        
        private void UpdateAttackWarningTransform()
        {
            if (triggerCollider == null || _attackWarningLine == null) return;

            var bounds = triggerCollider.bounds;
            var floorY = bounds.min.y + _warningFloorOffset;
            var center = bounds.center;
            var halfX = bounds.extents.x;
            var halfZ = bounds.extents.z;

            _attackWarningLine.SetPosition(0, new Vector3(center.x - halfX, floorY, center.z - halfZ));
            _attackWarningLine.SetPosition(1, new Vector3(center.x + halfX, floorY, center.z - halfZ));
            _attackWarningLine.SetPosition(2, new Vector3(center.x + halfX, floorY, center.z + halfZ));
            _attackWarningLine.SetPosition(3, new Vector3(center.x - halfX, floorY, center.z + halfZ));
        }
        private void ShowAttackWarning()
        {
            EnsureAttackWarningVisual();
            UpdateAttackWarningTransform();
            _attackWarningObject.SetActive(true);

            _attackWarningTween?.Kill();
            _attackWarningTween = DOTween.To(
                    () => _warningAlpha,
                    SetWarningAlpha,
                    endValue: 1f,
                    duration: _warningFadeInDuration)
                .SetEase(Ease.OutExpo) // резкий, "вспыхивающий" вход
                .SetTarget(this)
                .SetLink(gameObject);
        }

        private void HideAttackWarning()
        {
            if (_attackWarningObject == null || !_attackWarningObject.activeSelf) return;

            _attackWarningTween?.Kill();
            _attackWarningTween = DOTween.To(
                    () => _warningAlpha,
                    SetWarningAlpha,
                    endValue: 0f,
                    duration: _warningFadeOutDuration)
                .SetEase(Ease.InQuad)
                .SetTarget(this)
                .SetLink(gameObject)
                .OnComplete(() => _attackWarningObject.SetActive(false));
        }

        private void SetWarningAlpha(float alpha)
        {
            _warningAlpha = alpha;
            ApplyWarningColor(new Color(1f, 0f, 0f, alpha));
        }
        private void Update()
        {
            if (_attackWarningObject != null && _attackWarningObject.activeSelf)
                UpdateAttackWarningTransform();
        }

        private void OnDestroy()
        {
            _receivers.Clear();
            _attackWarningTween?.Kill();
            if (_attackWarningMaterial != null) Destroy(_attackWarningMaterial);
            if (_attackWarningObject != null) Destroy(_attackWarningObject);
        }
        
        #endregion
    }
}