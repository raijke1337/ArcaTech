using System.Collections.Generic;
using Arcatech.Triggers;
using UnityEngine;

namespace Arcatech.Items.Projectiles
{
    [CreateAssetMenu(fileName = "New Homing Projectile Behavior", menuName = "Projectiles/Behavior/Homing")]
    public class SerializedHomingProjectileBehavior : SerializedBasicProjectileBehavior
    {
        public float maxAngleAdjust = 15f;
        public override ProjectileBehavior Deserialize()
        {
            return new HomingProjectileBehavior(baseProjectileSettings);
        }
    }
    
    
    public class HomingProjectileBehavior : BaseProjectileBehavior
    {
        
        BaseGameEntityComponent target;

        float scanTimer = 0;
        float range;
        Collider[] scanResults;
        List<BaseGameEntityComponent> hitTarget = new();


        // protected override void Update()
        // {
        //     scanTimer += Time.deltaTime;
        //
        //     if (scanTimer > 0.5f) //TODO maybe
        //     {
        //         scanTimer = 0;
        //         if (target != null) return;
        //
        //         Physics.OverlapSphereNonAlloc(transform.position, range, scanResults);
        //         foreach (Collider col in scanResults)
        //         {
        //             if (col == null) return;
        //             if (col.TryGetComponent<BaseGameEntityComponent>(out var e) && e.GetEntitySide!= Owner.GetEntitySide&& !hitTarget.Contains(e))
        //             { 
        //                 target = e;                      
        //                 break;
        //             }
        //         }
        //     }
        //     if (target != null)
        //     {
        //         transform.LookAt(new Vector3 (target.transform.position.x, transform.position.y, target.transform.position.z));
        //     }
        //
        //     transform.position += Speed * Time.deltaTime * transform.forward;
        //     Lifetime -= Time.deltaTime;
        //     if (Lifetime < 0)
        //     {
        //         Destroy(gameObject);
        //     }
        // }


        public HomingProjectileBehavior(BaseProjectileSettings settings) : base(settings)
        {
        }

        public override void UpdatePosition(float delta, Transform projectileTransform)
        {
            throw new System.NotImplementedException();
        }
    }
}
