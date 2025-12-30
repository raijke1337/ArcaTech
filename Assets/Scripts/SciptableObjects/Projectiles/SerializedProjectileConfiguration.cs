using Arcatech.Actions;
using UnityEngine;
using UnityEngine.Assertions;
namespace Arcatech.Items.Projectiles
{
    [CreateAssetMenu(fileName = "projectile_", menuName = "Projectiles/Projectile")]
    public class SerializedProjectileConfiguration : ScriptableObject
    {
        [SerializeField] ProjectileComponent projectilePrefab;
        [SerializeField] SerializedProjectileBehavior projectileBehavior;


        public ProjectileComponent ProduceProjectile (BaseGameEntityComponent owner,int maxHits, Vector3 pos, Quaternion rot)
        {
            ProjectileComponent proj = Instantiate(projectilePrefab, pos, rot);
            Vector3 dir = owner.transform.forward;
            proj.transform.forward = dir;
            
            proj.Setup(owner,projectileBehavior,maxHits);
            
            return proj;
        }
    }

    public enum SpawningPlaceType
    {
        WeaponSpawner,
        WeaponParent,
        UnitEffectsSpawn,
    }
}