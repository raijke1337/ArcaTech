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
            
        }

        private void SetupOwnerCollider()
        {
            _ownerCollider = _owner.GetComponent<Collider>();
        }

        public void StartBeam(Vector3 direction)
        {
            _isActive = true;
            _raycastTimer = 0f;
            _lastHitDistances.Clear();
            _lineRenderer.enabled = true;
            
        }

        public void StopBeam()
        {
            _isActive = false;
            _lineRenderer.enabled = false;
            
        }

        private void LateUpdate()
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

            RaycastHit[] hits = Physics.RaycastAll(beamStart, beamDirection, _config.beamLength);


            // Sort hits by distance
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (RaycastHit hit in hits)
            {
                // Ignore owner's collider if configured
                if (hit.collider == _ownerCollider)
                {

                    continue;
                }

                BaseGameEntityComponent target = hit.collider.GetComponent<BaseGameEntityComponent>();

                // Check minimum distance requirement
                if (target != null && !CanHitTarget(target, hit.distance))
                {

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
        }

        public void UnregisterReceiver(ITriggerNotificationReceiver receiver)
        {
            _receivers.Remove(receiver);
        }
    }
}