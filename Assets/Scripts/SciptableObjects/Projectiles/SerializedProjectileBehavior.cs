using UnityEngine;

namespace Arcatech.Items.Projectiles
{
    public abstract class SerializedProjectileBehavior : ScriptableObject
    { 
        public abstract ProjectileBehavior Deserialize(BaseGameEntityComponent owner);
    }
    
    public abstract class ProjectileBehavior
    {
        protected BaseGameEntityComponent Owner;
        public bool BehaviorCompleted { get;protected set; }
        public abstract void UpdatePosition(float delta, Transform projectileTransform);
        public abstract void NotifyCollision(TriggerHitInfo hit);
        public abstract void Reset();
    }
    
    
    
}