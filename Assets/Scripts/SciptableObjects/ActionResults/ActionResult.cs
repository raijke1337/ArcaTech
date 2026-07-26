using UnityEngine;

namespace Arcatech.Actions
{
    public abstract class SerializedActionResult : ScriptableObject
    {
        public abstract ActionResult Deserialize();

    }

    // trigger toggle
    // stat change
    // spawn projectile 
    // movement impulse
    // spawn particle effect
    // enter a state
    // anything else?
    // also do actual input commands now
    public abstract class ActionResult
    {
        public abstract bool ProduceResult(BaseGameEntityComponent user, BaseGameEntityComponent target
            ,Vector3 place, Quaternion placeRot);
    }
}