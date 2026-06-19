using UnityEngine;

namespace Arcatech.Usables.Effects
{
    [CreateAssetMenu(fileName = "usableEffect_push_", menuName = "Usables/Applied Effects/Impulse")]
    public class AppliedImpulseEffect : BaseAppliedEffect
    {
        
    }

    public class ImpulseResult : IEffectResult
    {
        public ImpulseResult(AppliedImpulseEffect impulse)
        {
            
        }
        public void Apply(EffectContext ctx)
        {
            throw new System.NotImplementedException();
        }

        public void OnExpire(EffectContext ctx)
        {
            throw new System.NotImplementedException();
        }
    }
}