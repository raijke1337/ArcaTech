using UnityEngine;

namespace Arcatech.Items.Projectiles
{
    [CreateAssetMenu(fileName = "projectile_", menuName = "Projectiles/Projectile")]
    public class SerializedProjectileConfiguration : ScriptableObject
    {
        [SerializeField] ProjectileComponent projectilePrefab;
        [SerializeField] SerializedProjectileBehavior projectileBehavior;
        [SerializeField] ProjectileHitRules hitRules;

        public ProjectileHitRules HitRules => hitRules;

        public ProjectileComponent ProduceProjectile (BaseGameEntityComponent owner, Vector3 pos, Quaternion rot)
        {
            ProjectileComponent proj = Instantiate(projectilePrefab, pos, rot);
            Vector3 dir = owner.transform.forward;
            proj.transform.forward = dir;

            proj.Setup(owner, projectileBehavior, hitRules);

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
