using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Arcatech.Triggers;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Items
{
    [RequireComponent(typeof(ParticleSystem))]
    public class LaserEmitterComponent : RangedWeaponShooterComponent, ITriggerNotificationProvider
    {
        #region interface

        public bool Active
        {
            get => isLaserActive;
            set
            {
                if (value)
                    ActivateLaser();
            }
        }
        
        private List<ITriggerNotificationReceiver> receivers = new List<ITriggerNotificationReceiver>();
        
        public void RegisterReceiver(ITriggerNotificationReceiver receiver)
        {
            if (receivers == null)
                receivers = new List<ITriggerNotificationReceiver>();
            if (receivers.Contains(receiver)) return;
            receivers.Add(receiver);
        }

        public void UnregisterReceiver(ITriggerNotificationReceiver receiver)
        {
            if (!receivers.Contains(receiver)) return;
            receivers.Remove(receiver);
        }

        public void RecheckCollisions()
        {
            // not needed
        }

        private void NotifyTriggerEntered(BaseGameEntityComponent triggeredObject)
        {
            foreach (var receiver in receivers)
            {
                receiver?.TriggerEntered(triggeredObject, this);
            }
        }
    
        private void NotifyTriggerExited(BaseGameEntityComponent triggeredObject)
        {
            foreach (var receiver in receivers)
            {
                receiver?.TriggerExited(triggeredObject, this);
            }
        }
        #endregion

        public void FireLaser()
        {
            if (currentStrategy == null)
            {
                Debug.LogWarning("No beam strategy configured!");
                return;
            }

            if (laserCoroutine != null)
            {
                StopCoroutine(laserCoroutine);
                laserCoroutine = null;
            }
            laserCoroutine = StartCoroutine(LaserCoroutine(currentStrategy.BeamSettings.DefaultDuration));
        }
        public void ConfigureBeam(ShootBeamStrategy strat)
        {
            firePoint = strat.SpawnPoint;
            currentStrategy = strat;
            ConfigureLineRenderer();
        }
        
        private LineRenderer lineRenderer;
        private AudioSource audioSource;
        private Coroutine laserCoroutine;
        private HashSet<BaseGameEntityComponent> currentHits = new HashSet<BaseGameEntityComponent>();

        private ShootBeamStrategy currentStrategy;
        private bool isLaserActive = false;
        private float laserStartTime;
        private Transform firePoint;
        private float timeToRefresh = 0f;
        private void Awake()
        {
            // Setup LineRenderer
            lineRenderer = gameObject.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }
        
            // Setup AudioSource
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        
            lineRenderer.enabled = false;
            lineRenderer.useWorldSpace = true;
        }

        private void ConfigureLineRenderer()
        {
            if (currentStrategy == null) return;
        
            lineRenderer.material = currentStrategy.BeamSettings.LaserMaterial;
            lineRenderer.startColor = currentStrategy.BeamSettings.LaserColor;
            lineRenderer.startWidth = currentStrategy.BeamSettings.BeamWidth;
            lineRenderer.endWidth = currentStrategy.BeamSettings.BeamWidth;
            lineRenderer.positionCount = 2;
        }
        #region Laser Logic
    
        private IEnumerator LaserCoroutine(float duration)
        {
            ActivateLaser();


            bool useInfinite = duration < 0 || currentStrategy.BeamSettings.UseInfiniteDuration;
        
            if (useInfinite)
            {
                // Run until manually stopped
                while (isLaserActive)
                {
                    UpdateLaser();
                    yield return null;
                }
            }
            else
            {
                // Run for specified duration
                float elapsed = 0f;
                while (elapsed < duration && isLaserActive)
                {
                    UpdateLaser();
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }
        
            DeactivateLaser();
        }


        private void ActivateLaser()
        {
            if (currentStrategy == null) return;

            timeToRefresh = currentStrategy.BeamSettings.BurnIntervals;
            isLaserActive = true;
            laserStartTime = Time.time;
            lineRenderer.enabled = true;



            // Play audio
            if (currentStrategy.BeamSettings.FireSound != null)
            {
                audioSource.PlayOneShot(currentStrategy.BeamSettings.FireSound);
            }

            if (currentStrategy.BeamSettings.LoopSound != null)
            {
                audioSource.clip = currentStrategy.BeamSettings.LoopSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }

        private void DeactivateLaser()
        {
            isLaserActive = false;
            Active = false;
            lineRenderer.enabled = false;
        
            // Stop audio
            if (audioSource.isPlaying && audioSource.loop)
            {
                audioSource.Stop();
            }
        
            if (currentStrategy?.BeamSettings.StopSound != null)
            {
                audioSource.PlayOneShot(currentStrategy.BeamSettings.StopSound);
            }
        
            // Notify all currently hit entities that they've exited
            foreach (var entity in currentHits)
            {
                if (entity != null)
                {
                    NotifyTriggerExited(entity);
                }
            }

            Active = false;
            currentHits.Clear();
        }
        private void UpdateLaser()
        {
            if (!isLaserActive || currentStrategy == null) return;
        timeToRefresh -= Time.deltaTime;
        if (timeToRefresh <= 0f)
        {
            currentHits.Clear();
            timeToRefresh = currentStrategy.BeamSettings.BurnIntervals;
        }
            
            Vector3 startPoint = firePoint.position;
            Vector3 direction = firePoint.forward;
        
            // Update laser intensity based on curve
            float normalizedTime = currentStrategy.BeamSettings.UseInfiniteDuration ? 1f : 
                (Time.time - laserStartTime) / currentStrategy.BeamSettings.DefaultDuration;
            float intensity = currentStrategy.BeamSettings.IntensityCurve.Evaluate(normalizedTime);
            Color currentColor = currentStrategy.BeamSettings.LaserColor;
            currentColor.a = intensity;
            lineRenderer.startColor = currentColor;
        
            // Perform raycast
            RaycastHit hit;
            Vector3 endPoint;
        
            if (Physics.Raycast(startPoint, direction, out hit, currentStrategy.BeamSettings.MaxRange, currentStrategy.BeamSettings.CollisionMask))
            {
                endPoint = hit.point;
                HandleLaserHit(hit);
            }
            else
            {
                endPoint = startPoint + direction * currentStrategy.BeamSettings.MaxRange;
            }
        
            // Update line renderer
            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, endPoint);
        
            // Check for entities that are no longer being hit
            CheckForExitedEntities(startPoint, direction);
        }
        
        
        private void HandleLaserHit(RaycastHit hit)
        {
            BaseGameEntityComponent entity = hit.collider.GetComponent<BaseGameEntityComponent>();
        
            if (entity != null)
            {
                // Check if this is a new hit
                if (!currentHits.Contains(entity))
                {
                    currentHits.Add(entity);
                    NotifyTriggerEntered(entity);
                }
            }
        }
        private void CheckForExitedEntities(Vector3 startPoint, Vector3 direction)
        {
            List<BaseGameEntityComponent> entitiesToRemove = new List<BaseGameEntityComponent>();
        
            foreach (var entity in currentHits)
            {
                if (entity == null || !IsEntityInLaserPath(entity, startPoint, direction))
                {
                    entitiesToRemove.Add(entity);
                }
            }
        
            foreach (var entity in entitiesToRemove)
            {
                currentHits.Remove(entity);
                if (entity != null)
                {
                    NotifyTriggerExited(entity);
                }
            }
        }
        private bool IsEntityInLaserPath(BaseGameEntityComponent entity, Vector3 startPoint, Vector3 direction)
        {
            RaycastHit[] hits = Physics.RaycastAll(startPoint, direction, currentStrategy.BeamSettings.MaxRange, currentStrategy.BeamSettings.CollisionMask);
        
            foreach (var hit in hits)
            {
                if (hit.collider.GetComponent<BaseGameEntityComponent>() == entity)
                {
                    return true;
                }
            }
        
            return false;
        }
        #endregion
    
        #region Unity Events
    
        private void OnDisable()
        {
            DeactivateLaser();
        }
    
        private void OnDestroy()
        {
            DeactivateLaser();
            receivers.Clear();
        }
    
        #endregion
    }

}