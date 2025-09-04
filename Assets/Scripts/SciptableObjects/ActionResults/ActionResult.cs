using UnityEngine;

namespace Arcatech.Actions
{
    public abstract class SerializedActionResult : ScriptableObject
    {
        public abstract IActionResult BuildActionResult();

    }

    // trigger toggle
    // stat change
    // spawn projectile 
    // movement impulse
    // spawn particle effect
    // play an animation action
    public abstract class ActionResult : IActionResult
    {
        public abstract void ProduceResult(BaseGameEntityComponent user, BaseGameEntityComponent target,Transform place);
    }
}