using System;
using Arcatech.Actions;
using UnityEngine;
using UnityEngine.Assertions;
namespace Arcatech.Items.Projectiles
{
    [CreateAssetMenu(fileName = "New Projectile", menuName = "Projectiles/Projectile")]
    public class SerializedProjectileConfiguration : ScriptableObject
    {
        [SerializeField] ProjectileComponent projectilePrefab;
        [SerializeField] SerializedProjectileBehavior projectileBehavior;


        public ProjectileComponent ProduceProjectile (BaseGameEntityComponent owner, Vector3 pos, Quaternion rot,  float spread = 0f)
        {
            ProjectileComponent proj = Instantiate(projectilePrefab, pos, rot);
            Vector3 dir = owner.transform.forward + new Vector3(UnityEngine.Random.Range(-spread, spread), UnityEngine.Random.Range(-spread, spread), UnityEngine.Random.Range(-spread, spread));

            proj.transform.forward = dir;
            
            proj.Setup(owner,projectileBehavior);
            
            return proj;
        }
    }

    [Serializable]
    public struct ShootingConfig
    {
        public int Shots;
        public float Spread;
        public float BetweenShotsDelay;
        public float ShotDelay;
    }
}