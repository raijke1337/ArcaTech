using Arcatech.EventBus;
using Arcatech.Items;
using Arcatech.Units;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Actions
{
    [CreateAssetMenu(fileName = "New produce projectile result", menuName = "Actions/Action Result/PlaceProjectile", order = 1)]
    public class SerializedProduceProjectileResult : SerializedActionResult
    {
        [SerializeField] SerializedProjectileConfiguration Projectile;
        [SerializeField, Tooltip("seconds before projectile will spawn is done"),Range(0f, 1f)] float shotDelay = 0.3f;
        [SerializeField, Range(0, 10)] float spread;

        [Space,Header("Multi shots"),SerializeField, Range(1, 10)] int numberOfProjectiles;
        [SerializeField, Range(0.1f, 1f)] float BetweenShotsDelay = 0.1f;

        private void OnValidate()
        {
            Assert.IsNotNull(Projectile);
            Assert.IsFalse(numberOfProjectiles == 0);
        }
        public override IActionResult BuildActionResult()
        {
            return new ProduceProjectileResult(Projectile,numberOfProjectiles,spread,BetweenShotsDelay, shotDelay);
        }

        public override string ToString()
        {
            return $"projectile result : {Projectile}";
        }
    }
    public class ProduceProjectileResult : ActionResult
    {
        SerializedProjectileConfiguration _p;
        ShootingConfig _cfg;
        ProjectilePlaceEvent cachedEvent;
        public ProduceProjectileResult(SerializedProjectileConfiguration p, int n,float s, float d, float st)
        {
            _p = p;
            _cfg = new ShootingConfig(n, s, d, st);
            cachedEvent = new ProjectilePlaceEvent(null, null, _p, _cfg);
        }

        public override void ProduceResult(BaseEntity user, BaseEntity target, Transform place)
        {
            if (cachedEvent.Shooter != user)
            {
                cachedEvent.Shooter = user;
            }
            if (cachedEvent.Place != place)
            {
                cachedEvent.Place = place;
            }
            EventBus<ProjectilePlaceEvent>.Raise(cachedEvent);
        }
    }

    [Serializable]
    public struct ShootingConfig
    {
        public ShootingConfig(int shots, float spread, float delay, float shotDelay)
        {
            Shots = shots;
            Spread = spread;
            BetweenShotsDelay = delay;
            ShotDelay = shotDelay;
        }

        public int Shots { get; }
        public float Spread { get; }
        public float BetweenShotsDelay { get; }
        public float ShotDelay { get; }
    }

    public struct ProjectilePlaceEvent : IEvent
    {
        public BaseEntity Shooter;
        public Transform Place;
        public readonly SerializedProjectileConfiguration Projectile;
        public readonly ShootingConfig ShootingConfig;

        public ProjectilePlaceEvent(BaseEntity shooter, Transform place, SerializedProjectileConfiguration projectile, ShootingConfig shootingConfig)
        {
            Shooter = shooter;
            Place = place;
            Projectile = projectile;
            ShootingConfig = shootingConfig;
        }
    }

}