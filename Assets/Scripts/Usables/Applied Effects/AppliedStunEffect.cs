using Arcatech.Stats;
using UnityEngine;

namespace Arcatech.Usables.Effects
{
    [CreateAssetMenu(fileName = "usableEffect_stun_", menuName = "Usables/Applied Effects/Stun")]
    public class AppliedStunEffect : BaseAppliedEffect
    {
        [Header("Stun")]
        [Tooltip("Duration of the actual 'stunned' state applied per tick.")]
        public float stunSeconds = 1f;

        [Header("Stun Condition")]
        [Tooltip("Stun will apply if these conditions are satisfied")]public ConditionGroup stunConditions;
        
    }
    /// <summary>
    /// Applies a 'stunned' state to the target for stunSeconds on each tick.
    /// OneShot -> one stun. Repeating -> several overlapping stun windows.
    /// Repetition from new hits is gated by StackType.None in the resolver.
    /// </summary>
    public sealed class StunResult : BaseResult
    {
        private readonly float _stunSeconds;
        private readonly ConditionGroup _stunConditions;

        public StunResult(AppliedStunEffect cfg) : base(cfg)
        {
            _stunSeconds = Mathf.Max(0f, cfg.stunSeconds);
            _stunConditions = cfg.stunConditions;
        }

        public override void Apply(EffectContext ctx)
        {
            if (ctx.Target == null || !ctx.TargetReceiver.TryGetStatusReceiver(out var c)) return;
            c.ApplyStun(ctx.Instance.Key, _stunSeconds);
            
        }

        public override bool Validate(EffectContext ctx)
        {
            if (_stunConditions.statConditions.Count > 0)
            {
                if (!ctx.Target.TryGetComponent(out EntityStatsComponent stats))
                {
                    Debug.LogWarning($"{ctx.Target} has no stat to validate the stun! {ctx.Instance.Key}");
                    return false;
                }
                if (!stats.CheckStatsConditionGroup(_stunConditions)) return false;
            }
            return true;
        }
        public override void OnExpire(EffectContext ctx)
        {
            // The stun state has its own timer in the status component; effect end
            // does not force-clear it (lets the last stun window finish naturally).
            // If you want effect-end to cancel stun immediately, call rec.ClearStun(key) here.
        }
    }
}