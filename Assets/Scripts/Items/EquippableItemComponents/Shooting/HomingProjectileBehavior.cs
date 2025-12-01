using System.Collections.Generic;
using Arcatech.Triggers;
using UnityEngine;

namespace Arcatech.Items.Projectiles
{
    [CreateAssetMenu(fileName = "New Homing Projectile Behavior", menuName = "Projectiles/Behavior/Homing")]
    public class SerializedHomingProjectileBehavior : SerializedBasicProjectileBehavior
    {
        public float maxAngleAdjust = 15f;
        public override ProjectileBehavior Deserialize(BaseGameEntityComponent comp)
        {
            return null; //new HomingProjectileBehavior(baseProjectileSettings);
        }
    }
}
