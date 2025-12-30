using System;
using System.Collections.Generic;
using Arcatech.Triggers;
using Arcatech.Usables;
using UnityEngine;

namespace Arcatech.Items
{
    public class BeamWeaponComponent : MonoBehaviour, ITriggerNotificationProvider
    {
        private SerializedBeamShooterConfig _config;
        private BaseGameEntityComponent _owner;

        private LineRenderer _lineRenderer;
        private float _raycastTimer;
        private bool _isActive;

        private readonly HashSet<ITriggerNotificationReceiver> _receivers = new();
        private readonly Dictionary<BaseGameEntityComponent, float> _lastHitDistances = new();
        private Collider _ownerCollider;

        private Transform _spawnPoint;
        
        // DEBUG
        [SerializeField] private bool _showDebugInfo = true;
        
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
            
            Debug.Log($"[BeamWeapon] Initialize called. SpawnPoint: {_spawnPoint?.name ?? "NULL"}");
            if (_spawnPoint != null)
            {
                Debug.Log($"[BeamWeapon] SpawnPoint position: {_spawnPoint.position}, forward: {_spawnPoint.forward}");
            }
            
            SetupLineRenderer();
            SetupOwnerCollider();

            _raycastTimer = 0f;
            _isActive = false;
            _lastHitDistances.Clear();
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
            
            Debug.Log($"[BeamWeapon] LineRenderer setup complete. UseWorldSpace: {_lineRenderer.useWorldSpace}");
        }

        private void SetupOwnerCollider()
        {
            _ownerCollider = _owner.GetComponent<Collider>();
            Debug.Log($"[BeamWeapon] Owner collider: {(_ownerCollider != null ? _ownerCollider.name : "NOT FOUND")}");
        }

        public void StartBeam(Vector3 direction)
        {
            _isActive = true;
            _raycastTimer = 0f;
            _lastHitDistances.Clear();
            _lineRenderer.enabled = true;
            
            Debug.Log($"[BeamWeapon] StartBeam called. Direction: {direction}");
        }

        public void StopBeam()
        {
            _isActive = false;
            _lineRenderer.enabled = false;
            
            Debug.Log($"[BeamWeapon] StopBeam called");
        }

        private void Update()
        {
            if (!_isActive) return;
            
            _raycastTimer += Time.deltaTime;
            UpdateBeamVisuals();
            PerformRaycasts();
        }

        private void UpdateBeamVisuals()
        {
            if (_spawnPoint == null)
            {
                Debug.LogError("[BeamWeapon] SpawnPoint is NULL in UpdateBeamVisuals!");
                return;
            }

            Vector3 beamStart = _spawnPoint.position;
            Vector3 beamDirection = _spawnPoint.forward;
            Vector3 beamEnd = beamStart + beamDirection * _config.beamLength;

            _lineRenderer.SetPosition(0, beamStart);
            _lineRenderer.SetPosition(1, beamEnd);

            if (_showDebugInfo)
            {
                Debug.Log($"[BeamWeapon] Beam visual updated - Start: {beamStart}, End: {beamEnd}");
                Debug.DrawLine(beamStart, beamEnd, Color.red, 0.016f);
            }
        }

        private void PerformRaycasts()
        {
            if (_raycastTimer < _config.raycastFrequency)
            {
                return;
            }

            int raycasts = _config.raycastsPerFrame;
            while (raycasts > 0 && _raycastTimer >= _config.raycastFrequency)
            {
                PerformSingleRaycast();
                _raycastTimer -= _config.raycastFrequency;
                raycasts--;
            }
        }

        private void PerformSingleRaycast()
        {
            if (_spawnPoint == null)
            {
                Debug.LogError("[BeamWeapon] SpawnPoint is NULL in PerformSingleRaycast!");
                return;
            }

            Vector3 beamStart = _spawnPoint.position;
            Vector3 beamDirection = _spawnPoint.forward;

            if (_showDebugInfo)
            {
                Debug.Log($"[BeamWeapon] Raycast from {beamStart} in direction {beamDirection} for {_config.beamLength} units");
            }

            RaycastHit[] hits = Physics.RaycastAll(beamStart, beamDirection, _config.beamLength);

            if (_showDebugInfo)
            {
                Debug.Log($"[BeamWeapon] Raycast hit {hits.Length} objects");
            }

            // Sort hits by distance
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (RaycastHit hit in hits)
            {
                if (_showDebugInfo)
                {
                    Debug.Log($"[BeamWeapon] Hit object: {hit.collider.name}, Distance: {hit.distance}");
                }

                // Ignore owner's collider if configured
                if (hit.collider == _ownerCollider)
                {
                    if (_showDebugInfo)
                    {
                        Debug.Log($"[BeamWeapon] Ignoring hit on owner collider: {hit.collider.name}");
                    }
                    continue;
                }

                BaseGameEntityComponent target = hit.collider.GetComponent<BaseGameEntityComponent>();

                // Check minimum distance requirement
                if (target != null && !CanHitTarget(target, hit.distance))
                {
                    if (_showDebugInfo)
                    {
                        Debug.Log($"[BeamWeapon] Target {target.GetName} failed distance check");
                    }
                    continue;
                }

                TriggerHitInfo hitInfo = new TriggerHitInfo(
                    triggerNotificationProvider: this,
                    baseGameEntityComponent: target,
                    hitPosition: hit.point,
                    impactDirection: beamDirection,
                    hitNormal: hit.normal,
                    time: Time.time
                );

                if (_showDebugInfo)
                {
                    Debug.Log($"[BeamWeapon] Valid hit registered on {(target != null ? target.GetName : "environment")} at {hit.point}");
                }

                NotifyReceivers(hitInfo);

                // Update last hit distance for this target
                if (target != null)
                {
                    _lastHitDistances[target] = hit.distance;
                }
            }
        }

        private bool CanHitTarget(BaseGameEntityComponent target, float currentDistance)
        {
            if (!_lastHitDistances.TryGetValue(target, out float lastDistance))
            {
                return true;
            }

            bool canHit = Mathf.Abs(currentDistance - lastDistance) >= _config.minDistanceBetweenHits;
            if (_showDebugInfo && !canHit)
            {
                Debug.Log($"[BeamWeapon] Hit distance check failed: current={currentDistance}, last={lastDistance}, min required={_config.minDistanceBetweenHits}");
            }
            return canHit;
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
            _receivers.Add(receiver);
            Debug.Log($"[BeamWeapon] Receiver registered: {receiver.GetType().Name}");
        }

        public void UnregisterReceiver(ITriggerNotificationReceiver receiver)
        {
            _receivers.Remove(receiver);
            Debug.Log($"[BeamWeapon] Receiver unregistered: {receiver.GetType().Name}");
        }
    }
}