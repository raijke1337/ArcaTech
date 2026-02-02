using UnityEngine;
using System.Collections.Generic;
using System;
using Arcatech.Triggers;
using Arcatech.Usables; // For System.Array.Sort

namespace Arcatech.Items
{
    public class BeamWeaponComponent : MonoBehaviour, ITriggerNotificationProvider
    {
        // Nested class to encapsulate all state relevant to a single target being hit by the beam.
        private class TargetBeamState
        {
            public Collider Target; // Reference to the entity being tracked.
            public float ContinuousBeamTime; // Total time continuously in beam (or within grace period).

            public int
                LastReportedIntervalIndex; // Which hit interval was last reported (e.g., 0 for first 0.5s, 1 for second 0.5s).

            public float
                OutOfBeamGraceTimer; // Time elapsed since the target last left the beam (0 if currently in beam).

            public TriggerHitInfo LastTriggerHitInfo; // Most recent hit info for notification.

            public TargetBeamState(Collider target)
            {
                Target = target;
                ContinuousBeamTime = 0f;
                LastReportedIntervalIndex = -1; // -1 indicates no interval has been reported yet.
                OutOfBeamGraceTimer = 0f;
                // LastTriggerHitInfo is a struct and initialized to default, will be set on first hit.
            }

            /// <summary>
            /// Resets the state for a target after its grace period expires or beam stops.
            /// </summary>
            public void FullyReset()
            {
                ContinuousBeamTime = 0f;
                LastReportedIntervalIndex = -1;
                OutOfBeamGraceTimer = 0f;
                // LastTriggerHitInfo will eventually be overwritten if the target is hit again.
            }

            /// <summary>
            /// Marks the target as actively being hit again, clearing its grace timer.
            /// </summary>
            public void MarkInBeam()
            {
                OutOfBeamGraceTimer = 0f;
            }
        }

        private SerializedBeamShooterConfig _config;
        private BaseGameEntityComponent _owner;

        private LineRenderer _lineRenderer;
        private float _raycastAccumulator; // Accumulates deltaTime to reach raycastFrequency
        private bool _isActive;

        private readonly HashSet<ITriggerNotificationReceiver> _receivers = new();
        private Collider _ownerCollider;

        private Transform _spawnPoint;

        // --- NEW / MODIFIED FIELDS FOR CONTINUOUS HIT DETECTION ---
        private readonly Dictionary<Collider, TargetBeamState> _trackedTargets = new();

        private readonly HashSet<Collider>
            _targetsHitThisFrame = new(); // Populated by raycasts, cleared each LateUpdate

        private readonly List<Collider>
            _targetsToRemove = new(); // Used for safe removal from _trackedTargets
        // --- END NEW / MODIFIED FIELDS ---

        public bool Active
        {
            get => _isActive;
            set => _isActive = value;
        }

        public void Initialize(BaseGameEntityComponent owner, EquipmentComponent equipment,
            SerializedBeamShooterConfig config)
        {
            _owner = owner;
            _config = config;

            _spawnPoint = equipment.EffectSpawn;

            SetupLineRenderer();
            SetupOwnerCollider();

            _raycastAccumulator = 0f;
            _isActive = false;

            // Clear all tracking on initialize for a clean state
            _trackedTargets.Clear();
            _targetsHitThisFrame.Clear();
            _targetsToRemove.Clear();
        }

        private void SetupLineRenderer()
        {
            _lineRenderer = gameObject.GetComponent<LineRenderer>();
            if (_lineRenderer == null)
            {
                _lineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            _lineRenderer.material = _config.beamMaterial;
            _lineRenderer.startWidth = _config.beamWidth;
            _lineRenderer.endWidth = _config.beamWidth;
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.positionCount = 2;
            _lineRenderer.enabled = false;
        }

        private void SetupOwnerCollider()
        {
            _ownerCollider = _owner != null ? _owner.GetComponent<Collider>() : null;
        }

        public void StartBeam(Vector3 direction)
        {
            _isActive = true;
            _raycastAccumulator = 0f;
            _lineRenderer.enabled = true;

            // Clear all tracking on beam start for a fresh state
            _trackedTargets.Clear();
            _targetsHitThisFrame.Clear();
            _targetsToRemove.Clear();
        }

        public void StopBeam()
        {
            _isActive = false;
            _lineRenderer.enabled = false;

            // Clear all tracking on beam stop
            _trackedTargets.Clear();
            _targetsHitThisFrame.Clear();
            _targetsToRemove.Clear();
        }

        private void LateUpdate()
        {
            if (!_isActive) return;

            _targetsHitThisFrame.Clear(); // Clear the set of targets hit during this frame's raycasts

            _raycastAccumulator += Time.deltaTime;
            UpdateBeamVisuals();
            PerformRaycasts();

            ProcessBeamTracking(); // Handle continuous time, grace periods, and notifications
        }

        private void UpdateBeamVisuals()
        {
            if (_spawnPoint == null)
            {
                // Debug.LogError may be too spammy if this happens often. Consider a warning or fail silently.
                return;
            }

            Vector3 beamStart = _spawnPoint.position;
            Vector3 beamDirection = _spawnPoint.forward;
            Vector3 beamEnd = beamStart + beamDirection * _config.beamLength;

            _lineRenderer.SetPosition(0, beamStart);
            _lineRenderer.SetPosition(1, beamEnd);
        }

        private void PerformRaycasts()
        {
            if (_config.raycastFrequency <= 0) // If frequency is zero or negative, raycast once per frame
            {
                PerformSingleRaycast(_spawnPoint.position, _spawnPoint.forward);
            }
            else
            {
                // Perform raycasts as long as accumulator has enough time
                while (_raycastAccumulator >= _config.raycastFrequency)
                {
                    for (int i = 0; i < _config.raycastsPerFrame; i++)
                    {
                        PerformSingleRaycast(_spawnPoint.position, _spawnPoint.forward);
                    }

                    _raycastAccumulator -= _config.raycastFrequency;
                }
            }
        }

        private void PerformSingleRaycast(Vector3 beamStart, Vector3 beamDirection)
        {
            if (_spawnPoint == null)
            {
                Debug.LogError("[BeamWeapon] SpawnPoint is NULL in PerformSingleRaycast!");
                return;
            }

            // Use RaycastAll to detect all potential targets along the beam's path.
            RaycastHit[] hits = Physics.RaycastAll(beamStart, beamDirection, _config.beamLength);

            // Sort hits by distance to ensure closest hit info is potentially prioritized if multiple colliders on one entity.
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (RaycastHit hit in hits)
            {
                // Ignore owner's collider to prevent self-hitting
                if (_ownerCollider != null && hit.collider == _ownerCollider)
                {
                    continue;
                }

                var hitCollider = hit.collider;

                if (hitCollider != null)
                {
                    // Add the target to the set of entities actually hit by raycasts this frame
                    _targetsHitThisFrame.Add(hitCollider);

                    // Get or create the TargetBeamState for this entity
                    if (!_trackedTargets.TryGetValue(hitCollider, out TargetBeamState state))
                    {
                        state = new TargetBeamState(hitCollider);
                        _trackedTargets.Add(hitCollider, state);
                    }

                    // Always update the LastTriggerHitInfo with the most recent hit details
                    state.LastTriggerHitInfo = new TriggerHitInfo(
                        triggerNotificationProvider: this,
                        hit: hit.collider,
                        hitPosition: hit.point,
                        impactDirection: beamDirection,
                        hitNormal: hit.normal,
                        time: Time.time
                    );

                    // Mark the target as currently in beam, resetting any active grace period timer
                    state.MarkInBeam();
                }
            }
        }

        private void ProcessBeamTracking()
        {
            float deltaTime = Time.deltaTime;
            _targetsToRemove.Clear(); // Clear list for this frame's removals

            // Iterate through all currently tracked targets to update their state
            foreach (var entry in _trackedTargets)
            {
                Collider target = entry.Key;
                TargetBeamState state = entry.Value;

                if (_targetsHitThisFrame.Contains(target))
                {
                    // Target is actively being hit by raycasts this frame
                    state.ContinuousBeamTime += deltaTime;
                    state.OutOfBeamGraceTimer = 0f; // Ensure grace timer is explicitly zero

                    // Check if a new hit interval has been reached
                    if (_config.interval > 0) // Avoid division by zero
                    {
                        int currentIntervalIndex = Mathf.FloorToInt(state.ContinuousBeamTime / _config.interval);

                        if (currentIntervalIndex > state.LastReportedIntervalIndex)
                        {
                            // A new interval has passed, report a hit
                            NotifyReceivers(state.LastTriggerHitInfo);
                            state.LastReportedIntervalIndex = currentIntervalIndex;
                        }
                    }
                    else // If interval is zero or negative, it's an "instant" hit once when first detected
                    {
                        if (state.LastReportedIntervalIndex < 0) // Report only once for initial detection
                        {
                            NotifyReceivers(state.LastTriggerHitInfo);
                            state.LastReportedIntervalIndex = 0; // Mark as reported
                        }
                    }
                }
                else
                {
                    // Target was tracked but is NOT in the beam this frame, start or continue grace period
                    state.OutOfBeamGraceTimer += deltaTime;

                    // Check if the grace period has expired (negative gracePeriod implies infinite grace)
                    if (_config.gracePeriod >= 0 && state.OutOfBeamGraceTimer > _config.gracePeriod)
                    {
                        // Grace period expired, target is no longer continuously hit. Mark for removal.
                        _targetsToRemove.Add(target);
                    }
                }
            }

            // Perform actual removals after iteration to avoid modifying collection during enumeration
            foreach (Collider target in _targetsToRemove)
            {
                // Remove from _trackedTargets
                _trackedTargets.Remove(target);
                // The TargetBeamState object becomes eligible for garbage collection.
            }
        }

        private void NotifyReceivers(TriggerHitInfo hitInfo)
        {
            foreach (ITriggerNotificationReceiver receiver in _receivers)
            {
                receiver.TriggerEntered(hitInfo);
            }
        }

        public void RegisterReceiver(ITriggerNotificationReceiver receiver)
        {
            if (receiver != null)
            {
                _receivers.Add(receiver);
            }
        }

        public void UnregisterReceiver(ITriggerNotificationReceiver receiver)
        {
            if (receiver != null)
            {
                _receivers.Remove(receiver);
            }
        }
    }
}