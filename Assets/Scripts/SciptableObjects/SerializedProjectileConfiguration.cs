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
}