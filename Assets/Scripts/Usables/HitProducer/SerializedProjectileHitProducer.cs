using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Arcatech.Actions;
using Arcatech.Items;
using Arcatech.Items.Projectiles;
using Arcatech.Triggers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;

namespace Arcatech.Usables
{
    /// <summary>
    /// uses projectiles to report hits
    /// </summary>
    [CreateAssetMenu(fileName = "New Projectiles Hit Producer", menuName = "Usables/Hit Producer/Projectile")]
    public class SerializedProjectileHitProducer : SerializedHitProducer
    {
        [SerializeField] private SerializedProjectileConfiguration projectile;
        [SerializeField] private ShootingConfig projectileShootingConfig;
        [Min(0)] public int projectilePoolSize = 32;

        public override IHitProducer Deserialize(BaseGameEntityComponent owner, EquipmentComponent item)
        {
            return new ProjectileHitProducer(owner, item,projectile,projectileShootingConfig,projectilePoolSize);
        }
    }

    public class ProjectileHitProducer : IHitProducer
    {
        
        private SerializedProjectileConfiguration _projectile;
        private ShootingConfig _shooting;
        private EquipmentComponent _item;
        private BaseGameEntityComponent _owner;
        private Coroutine _shootingCor;
        
        private IObjectPool<ProjectileComponent> _projectilePool;
        private List<ProjectileComponent> _activeProjectiles = new List<ProjectileComponent>();
        
        public ProjectileHitProducer(BaseGameEntityComponent owner, EquipmentComponent item,SerializedProjectileConfiguration proj, ShootingConfig shooting, int size)
        {
            _projectile = proj;
            _shooting = shooting;
            _owner = owner;
            _item = item;
            
            _projectilePool = new ObjectPool<ProjectileComponent>(
                createFunc: CreateProjectile,
                actionOnGet: OnProjectileGet,
                actionOnRelease: OnProjectileRelease,
                actionOnDestroy: OnProjectileDestroy,
                collectionCheck: true,
                defaultCapacity: size,
                maxSize: size * 2  // Allow pool to grow if needed
            );
        }
        private ProjectileComponent CreateProjectile()
        {
            // Create a new projectile instance
            var projectile = _projectile.ProduceProjectile(_owner, Vector3.zero, Quaternion.identity);
            return projectile;
        }
        private void OnProjectileGet(ProjectileComponent projectile)
        {
            // Reset projectile state when retrieved from pool
           // projectile.Entity.Killed = false;
            projectile.gameObject.SetActive(true);
            _activeProjectiles.Add(projectile);
        }
        private void OnProjectileRelease(ProjectileComponent projectile)
        {
            // Clean up projectile when returned to pool
            projectile.UnregisterReceiver(this);
            projectile.ProjectileExpiredEvent -= HandleProjectileExpiry;
            projectile.gameObject.SetActive(false);
            _activeProjectiles.Remove(projectile);
        }
        private void OnProjectileDestroy(ProjectileComponent projectile)
        {
            // Destroy the projectile GameObject when pool is destroyed
            if (projectile != null && projectile.gameObject != null)
            {
                projectile.Entity.Killed = true;
            }
        }
        private IEnumerator ShootingCoroutine()
        {

            if (_shooting.ShotDelay > 0)
            {
                yield return new WaitForSeconds(_shooting.ShotDelay);
            }

            int done = 0;
            while (done < _shooting.Shots)
            {
                done++;
        
                // Get projectile from pool
                var projectile = _projectilePool.Get();
                projectile.Reset();
                // Position and rotate it
                
                Vector3 place = _item.EffectSpawn.position;
                Quaternion rotation = _item.EffectSpawn.rotation;

                projectile.transform.position = place;
                projectile.transform.rotation = rotation;
        
                // Register for events
                projectile.RegisterReceiver(this);
                projectile.ProjectileExpiredEvent += HandleProjectileExpiry;
        
                yield return new WaitForSeconds(_shooting.BetweenShotsDelay);
            }
        }

        private void HandleProjectileExpiry(ProjectileComponent projectile)
        {
            // Return projectile to pool instead of destroying
            _projectilePool.Release(projectile);
        }


        public void Initialize()
        {
            _item.gameObject.SetActive(true); //bandaid
            _shootingCor = _item.StartCoroutine(ShootingCoroutine());
        }

        public event UnityAction<TriggerHitInfo> Hit;

        public void TriggerEntered(TriggerHitInfo triggerHitInfo)
        {
            Hit?.Invoke(triggerHitInfo);
        }

        public void TriggerExited(BaseGameEntityComponent exitComponent, ITriggerNotificationProvider trigger)
        { }
        public void Cleanup()
        {
            // Stop the shooting coroutine
            if (_shootingCor != null)
            {
                _item.StopCoroutine(_shootingCor);
            }

            // Return all active projectiles to the pool
            foreach (var projectile in _activeProjectiles.ToList())
            {
                _projectilePool.Release(projectile);
            }
    
            _activeProjectiles.Clear();
        }
    }
}