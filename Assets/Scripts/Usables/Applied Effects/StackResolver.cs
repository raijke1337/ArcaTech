using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Usables.Effects
{
    public enum StackDecision { Add, Refresh, Reject }

    public interface IStackingResolver
    {
        /// <summary>
        /// Decides what to do with an incoming effect given the instances
        /// already present under the SAME EffectKey (same effectId + same source).
        /// </summary>
        StackDecision Resolve(
            IReadOnlyList<ActiveEffectInstance> existingSameKey,
            BaseAppliedEffect.StackType stackType,
            int maxStacks);
    }

    public sealed class StackingResolver : IStackingResolver
    {
        public StackDecision Resolve(IReadOnlyList<ActiveEffectInstance> existing,
            BaseAppliedEffect.StackType stackType, int maxStacks)
        {
            int liveCount = existing?.Count ?? 0;

            switch (stackType)
            {
                case BaseAppliedEffect.StackType.None:
                    // design doc (stun): while active, new effects of this ID are not applied
                    return liveCount > 0 ? StackDecision.Reject : StackDecision.Add;

                case BaseAppliedEffect.StackType.Refresh:
                    // design doc (shield): repetition resets lifetime, no new instance
                    return liveCount > 0 ? StackDecision.Refresh : StackDecision.Add;

                case BaseAppliedEffect.StackType.Independent:
                    // design doc (stat change): each application stacks independently up to a cap
                    return liveCount >= Mathf.Max(1, maxStacks) ? StackDecision.Reject : StackDecision.Add;

                default:
                    return StackDecision.Add;
            }
        }
    }
}