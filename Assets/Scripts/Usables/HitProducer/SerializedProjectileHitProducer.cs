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
    /// uses projectiles to report hits.
    /// ВАЖНО: поле maxValidHitsPerUse здесь НЕ используется. Лимит поражаемых целей задаётся
    /// на снаряд: SerializedProjectileConfiguration -> Hit Rules -> Max Targets.
    /// </summary>
    [CreateAssetMenu(fileName = "hitProducer_projectile_", menuName = "Usables/Hit Producer/Projectile")]
    public class SerializedProjectileHitProducer : SerializedHitProducer
    {
        [SerializeField] public SerializedProjectileConfiguration projectile;
        [SerializeField] public ShootingConfig projectileShootingConfig;
        [Min(0)] public int projectilePoolSize = 32;

        public override IHitProducer Deserialize(BaseGameEntityComponent owner, EquipmentComponent item, bool indicateHitBox)
        {
            return new ProjectileHitProducer(owner, item,this,indicateHitBox);
        }
    }


    public class ProjectileHitProducer : HitProducer, IKillerComponent
    {
        public string KilledBy => "Hits producer";
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
            SerializedProjectileHitProducer config,bool indicateHitBox) : base(owner, item, config,indicateHitBox)
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
            //MaxHits *= _shooting.TotalBursts;

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
            var projectile = _projectile.ProduceProjectile(Owner, Vector3.zero, Quaternion.identity);
            // объект принадлежит этому пулу всю жизнь -> подписываемся один раз
            projectile.ProjectileFinished += HandleProjectileFinished;
            projectile.ProjectileHit += HandleProjectileHit;
            return projectile;
        }

        private void OnProjectileGet(ProjectileComponent projectile)
        {
            projectile.Reset();
            projectile.gameObject.SetActive(true);
            projectile.Active = true;
            _activeProjectiles.Add(projectile);
        }

        private void HandleProjectileFinished(ProjectileComponent projectile, ProjectileFinishReason reason)
        {
            // ProjectileComponent.Finish() идемпотентна, двойного Release не будет
            _projectilePool.Release(projectile);
        }

        private void OnProjectileRelease(ProjectileComponent projectile)
        {
            projectile.Active = false;
            projectile.gameObject.SetActive(false);
            _activeProjectiles.Remove(projectile);
        }

        private void OnProjectileDestroy(ProjectileComponent projectile)
        {
            if (projectile != null && projectile.gameObject != null)
            {
                projectile.ProjectileFinished -= HandleProjectileFinished;
                projectile.ProjectileHit -= HandleProjectileHit;
                projectile.Entity.SetKilled(this, true);
            }
        }

        /// <summary>
        /// Снаряд уже разобрал столкновение по правилам пробития (стена / владелец / повтор / лимит),
        /// здесь остаётся только превратить его в события применения.
        /// </summary>
        private void HandleProjectileHit(ProjectileComponent projectile, ProjectileCollision collision)
        {
            if (collision.IsEnvironment)
            {
                RaiseEnvironmentHit(collision.Hit);
                // TODO: legacy - AoE-аппликаторы (ракета) должны срабатывать и по стене, а раньше
                // это работало через EntityHit без цели. Заменить флагом AppliesOnEnvironment у аппликатора.
                RaiseEntityHit(collision.Hit);
                return;
            }

            if (collision.Accepted) RaiseEntityHit(collision.Hit);
        }

        /// <summary>
        /// Предмет снят: останавливает стрельбу, возвращает в пул летящие снаряды и уничтожает пул.
        /// Пул создаётся заново при следующем Get(), так что после повторной экипировки всё работает.
        /// </summary>
        public override void Detach()
        {
            if (_shootingCor != null && Owner != null) Owner.StopCoroutine(_shootingCor);
            _shootingCor = null;

            // Cancel() возвращает снаряд в пул и меняет _activeProjectiles - работаем с копией
            foreach (var projectile in _activeProjectiles.ToArray())
            {
                if (projectile != null) projectile.Cancel();
            }

            _projectilePool.Clear();
        }

        private IEnumerator ShootingCoroutine()
        {
            int done = 0;

            while (done < _shooting.TotalBursts)
            {
                yield return new WaitForEndOfFrame();
                done++;

                Vector3 centerPlace;
                var baseRot = Owner.transform.rotation;
        
                switch (_shooting.placeType)
                {
                    case SpawningPlaceType.WeaponSpawner:
                        centerPlace = Item.EffectSpawn.position;
                        break;
                    case SpawningPlaceType.WeaponParent:
                        centerPlace = Item.transform.parent.position;
                        break;
                    case SpawningPlaceType.UnitEffectsSpawn:
                        centerPlace = Owner.EffectSpawn.position;
                        break;
                    default:
                        Debug.Log("Unknown spawning place type");
                        centerPlace = Vector3.zero;
                        break;
                }

                // Spawn multiple projectiles in a ring
                foreach (var rot in _placementStrategy.GetRotations(baseRot, _shooting))
                {
                    var projectile = _projectilePool.Get(); // Reset/Active выставляются в OnProjectileGet
                    projectile.transform.SetPositionAndRotation(centerPlace, rot);
                }

                yield return new WaitForSeconds(_shooting.BetweenBurstsDelay);
            }
        }
    

    public override void OnChangeUsableState(StateMachineNotifyType info)
        {
            base.OnChangeUsableState(info);
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
                    if (_shootingCor != null) Owner.StopCoroutine(_shootingCor);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(info), info, null);
            }
        }
    }
}
