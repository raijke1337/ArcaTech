using System;
using System.Collections;
using System.Collections.Generic;
using Arcatech.Items;
using Arcatech.Items.Projectiles;
using Arcatech.Triggers;
using Arcatech.Units;
using UnityEngine;
using UnityEngine.Pool;

namespace Arcatech.Usables
{
    /// <summary>
    /// uses projectiles to report hits
    /// </summary>
    [CreateAssetMenu(fileName = "hitProducer_", menuName = "Usables/Hit Producer/Projectile")]
    public class SerializedProjectileHitProducer : SerializedHitProducer
    {
        [SerializeField] public SerializedProjectileConfiguration projectile;
        [SerializeField] public ShootingConfig projectileShootingConfig;
        [Min(0)] public int projectilePoolSize = 32;
        public override IHitProducer Deserialize(BaseGameEntityComponent owner, EquipmentComponent item)
        {
            return new ProjectileHitProducer(owner, item,this);
        }
    }


    public class ProjectileHitProducer : HitProducer
    {

        private SerializedProjectileConfiguration _projectile;
        private ShootingConfig _shooting;
        private Coroutine _shootingCor;

        private ISpreadStrategy _placementStrategy;


        private IObjectPool<ProjectileComponent> _projectilePool;

        /// <summary>
        /// component, current hits
        /// </summary>
        private List<ProjectileComponent> _activeProjectiles = new();

        public ProjectileHitProducer(BaseGameEntityComponent owner, EquipmentComponent item,
            SerializedProjectileHitProducer config) : base(owner, item, config)
        {
            _projectile = config.projectile;
            _shooting = config.projectileShootingConfig;

            _projectilePool = new ObjectPool<ProjectileComponent>(
                createFunc: CreateProjectile,
                actionOnGet: OnProjectileGet,
                actionOnRelease: OnProjectileRelease,
                actionOnDestroy: OnProjectileDestroy,
                collectionCheck: true,
                defaultCapacity: config.projectilePoolSize,
                maxSize: config.projectilePoolSize * 2 // Allow pool to grow if needed
            );
            MaxHits *= _shooting.TotalBursts;

            switch (_shooting.Pattern)
            {
                case PatternType.Single:
                    _placementStrategy = new SingleSpread();
                    break;
                case PatternType.Arc:
                    _placementStrategy = new EvenArcSpread();
                    break;
                case PatternType.Ring:
                    _placementStrategy = new RingSpread();
                    break;
                case PatternType.Cone:
                    _placementStrategy = new RandomConeSpread();
                    break;
                default:
                    Debug.LogError("Undefined pattern type");
                    break;
            }
        }

        private ProjectileComponent CreateProjectile()
        {
            // Create a new projectile instance
            var projectile = _projectile.ProduceProjectile(Owner, MaxHits, Vector3.zero, Quaternion.identity);
            return projectile;
        }

        private void OnProjectileGet(ProjectileComponent projectile)
        {
            // Reset projectile state when retrieved from pool
            projectile.Reset();
            // projectile.gameObject.SetActive(true);  
            _activeProjectiles.Add(projectile);
        }

        private void HandleProjectileExpiry(ProjectileComponent projectile)
        {
            // Return projectile to pool instead of destroying
            _projectilePool.Release(projectile);
        }

        private void OnProjectileRelease(ProjectileComponent projectile)
        {
            // Clean up projectile when returned to pool
            projectile.UnregisterReceiver(this);
            projectile.ProjectileFinished -= HandleProjectileExpiry;
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
            int done = 0;

            while (done < _shooting.TotalBursts)
            {
                done++;

                // 1) Decide place/rotation once per "shot group" (burst element)
                Vector3 place;
                var baseRot = Owner.transform.rotation;
                switch (_shooting.placeType)
                {
                    case SpawningPlaceType.WeaponSpawner:
                        place = Item.EffectSpawn.position;
                        break;
                    case SpawningPlaceType.WeaponParent:
                        place = Item.transform.parent.position;
                        break;
                    case SpawningPlaceType.UnitEffectsSpawn:
                        place = Owner.EffectSpawn.position;
                        break;
                    default:
                        Debug.Log("Unknown spawning place type");
                        place = Vector3.zero;
                        break;
                }

                // 2) Spawn multiple pellets simultaneously
                foreach (var rot in _placementStrategy.GetRotations(baseRot, _shooting))
                {
                    var projectile = _projectilePool.Get();
                    projectile.Reset();

                    // Optional slight positional jitter to avoid exact overlap
                    Vector3 offset = Vector3.zero;
                    if (_shooting.PelletSpawnRadius > 0f)
                    {
                        // small radial offset in plane perpendicular to forward
                        Vector2 circle = UnityEngine.Random.insideUnitCircle * _shooting.PelletSpawnRadius;
                        var right = rot * Vector3.right;
                        var up = rot * Vector3.up;
                        offset = right * circle.x + up * circle.y;
                    }

                    projectile.transform.SetPositionAndRotation(place + offset, rot);

                    projectile.gameObject.SetActive(true);
                    projectile.RegisterReceiver(this);
                    projectile.ProjectileFinished += HandleProjectileExpiry;
                }

                yield return new WaitForSeconds(_shooting.BetweenBurstsDelay);
            }
        }
    

    public override void OnChangeState(StateMachineNotifyType info)
        {
            base.OnChangeState(info);
            switch (info)
            {
                case StateMachineNotifyType.NoNotify:
                    break;
                case StateMachineNotifyType.Starting:
                    break;
                case StateMachineNotifyType.Use:
                    _shootingCor = Owner.StartCoroutine(ShootingCoroutine());
                    break;
                case StateMachineNotifyType.EndUse:
                    break;
                case StateMachineNotifyType.Cancel:
                    if (_shootingCor != null) Item.StopCoroutine(_shootingCor);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(info), info, null);
            }
        }
    

    public override void TriggerExited(TriggerHitInfo triggerExitInfo)
        { }
    }
}