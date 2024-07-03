using Arcatech.Triggers;
using Arcatech.Units;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace Arcatech.Items
{
    [CreateAssetMenu(fileName = "New Projectile", menuName = "Items/Projectile")]
    public class SerializedProjectileConfiguration : ScriptableObject
    {

        [SerializeField] ProjectileComponent ProjectilePrefab;
        [SerializeField] float TimeToLive;
        [SerializeField] float ProjectileSpeed;
        [Range(1, 10), Tooltip("How many enemies will be hit by this projectile"),SerializeField] int ProjectilePenetration;

        public ProjectileComponent ProduceProjectile(DummyUnit owner, Transform place, SerializedStatsEffectConfig[] effects)
        {
            var proj = Instantiate(ProjectilePrefab);
            proj.Owner = owner;
            proj.transform.forward = owner.transform.forward;
            proj.transform.SetPositionAndRotation(place.position, place.rotation);
            proj.Lifetime = TimeToLive;
            proj.RemainingHits = ProjectilePenetration; 
            proj.Speed = ProjectileSpeed;

            proj.AddEffects(effects);

            return proj;

        }

    }

}