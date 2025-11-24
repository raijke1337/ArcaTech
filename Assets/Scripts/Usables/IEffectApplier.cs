using System.Collections.Generic;
using Arcatech.Actions;
using UnityEngine;

namespace Arcatech.Usables
{
    // apply directly, in aoe or to self maybe?
    public interface IEffectApplier
    {
        void ApplyEffects(BaseGameEntityComponent user, TriggerHitInfo hit, List<ActionResult> effects, Vector3 origin);
    }

    public abstract class SerializedEffectApplier : ScriptableObject
    {
        public abstract IEffectApplier Deserialize();
    }


}