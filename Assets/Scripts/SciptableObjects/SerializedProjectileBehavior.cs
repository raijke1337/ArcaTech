using UnityEngine;

namespace Arcatech.Items.Projectiles
{
    public abstract class SerializedProjectileBehavior : ScriptableObject
    {
        // placeholder
        // homing, direct, bounc, boomerang.... whatever.
        public abstract ProjectileBehavior Deserialize();
    }
    
    public abstract class ProjectileBehavior
    {
        public bool IsExpired { get;protected set; }
        public abstract void UpdatePosition(float delta, Transform projectileTransform);
        public abstract void NotifyCollision(Collider collider);
        public abstract void Reset();
    }
}