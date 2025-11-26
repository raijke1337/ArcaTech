using System.Collections.Generic;
using Arcatech.Actions;
using UnityEditor.Search;
using UnityEngine;

namespace Arcatech.Usables
{
    // apply directly, in aoe or to self maybe?
    public interface IEffectApplier
    {
        void Rearm();
        void ApplyEffects(BaseGameEntityComponent user, TriggerHitInfo hit, List<ActionResult> effects, Vector3 origin);
    }

    public abstract class SerializedEffectApplier : ScriptableObject
    {
        public abstract IEffectApplier Deserialize();
    }

    public abstract class EffectApplier : IEffectApplier
    {
        private bool fired = false;
        public void Rearm() => fired = false;
        public void ApplyEffects(BaseGameEntityComponent user, TriggerHitInfo hit, List<ActionResult> effects, Vector3 origin)
        {
            if (fired) return;
            if (!hit.IsValidHit) return;
            fired =  true;
            DoApplyLogic(user, hit, effects, origin);
        }
        protected abstract void DoApplyLogic(BaseGameEntityComponent user, TriggerHitInfo hit, List<ActionResult> effects,Vector3 origin);
    }
}