using System.Collections.Generic;
using Arcatech.Actions;
using UnityEditor.Search;
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

    public abstract class EffectApplier : IEffectApplier
    {
        public void ApplyEffects(BaseGameEntityComponent user, TriggerHitInfo hit, List<ActionResult> effects, Vector3 origin)
        {
            if (!hit.IsValidHit) return;
            DoApplyLogic(user, hit, effects, origin);
        }
        protected abstract void DoApplyLogic(BaseGameEntityComponent user, TriggerHitInfo hit, List<ActionResult> effects,Vector3 origin);
    }
}