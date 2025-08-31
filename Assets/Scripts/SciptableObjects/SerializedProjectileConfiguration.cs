using Arcatech.Actions;
using System;
using UnityEngine;
using UnityEngine.Assertions;
namespace Arcatech.Items
{
    [CreateAssetMenu(fileName = "New Projectile", menuName = "Items/Projectile")]
    public class SerializedProjectileConfiguration : ScriptableObject
    {

        [SerializeField] ProjectileComponent ProjectilePrefab;
        [SerializeField] TargetingType AffectedTargets;
        [Range(1, 10), Tooltip("How many times affected targets will trigger the collision results"), SerializeField] int AffectedTargetsCount;
        [SerializeField] SerializedActionResult[] UnitCollisionResult;


        [Space,Header("Projectile")]
        [SerializeField] float TimeToLive;
        [SerializeField] float ProjectileSpeed;
        [SerializeField] bool attachToUser = false;

        [Header("TODO: repalce with homing settings so")]
        [SerializeField, Tooltip("Placeholder for homing projectiles, range of scanning for tgts")] float HomingRange = 6f;



        [SerializeField] SerializedActionResult[] ExpirationCollisionResult;



        private void OnValidate()
        {
            Assert.IsNotNull(ProjectilePrefab);
            Assert.IsNotNull(UnitCollisionResult);
            Assert.IsFalse(AffectedTargets == TargetingType.None);
        }
        /// <summary>
        /// instantiate the prefab and set it
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="place"></param>
        /// <param name="spread">in euler degrees</param>
        /// <returns></returns>
        //public virtual ProjectileComponent ProduceProjectile(BaseEntity owner, Transform place ,float spread = 0f)
        //{
        //    return ProduceProjectile(owner, place.position, place.rotation, attachToUser, spread);
        //}

        public virtual ProjectileComponent ProduceProjectile (ActiveGameUnitComponent owner, Vector3 pos, Quaternion rot,  float spread = 0f)
        {
            ProjectileComponent proj = Instantiate(ProjectilePrefab, pos, rot);
            proj.Owner = owner;
            Vector3 dir = owner.transform.forward + new Vector3(UnityEngine.Random.Range(-spread, spread), UnityEngine.Random.Range(-spread, spread), UnityEngine.Random.Range(-spread, spread));

            proj.transform.forward = dir;

            proj.Lifetime = TimeToLive;
            proj.RemainingHits = AffectedTargetsCount;
            proj.Speed = ProjectileSpeed;

            proj.SetResult(UnitCollisionResult, ExpirationCollisionResult,AffectedTargets);

            if (proj is HomingProjectileComponent h)
            {
                h.WithHoming(HomingRange);
            }

            if (attachToUser)
            {
                proj.Speed = 0;
                proj.transform.SetParent(owner.transform, true);
            }
            return proj;
        }
    }

}