using UnityEngine;

namespace Arcatech.Usables.Effects
{
    [CreateAssetMenu(fileName = "usableEffect_stun_", menuName = "Usable/Applied Effects/Stun")]
    public class AppliedStunEffect : BaseAppliedEffect
    {
        [Header("Stun")]
        [Tooltip("Duration of the actual 'stunned' state applied per tick.")]
        public float stunSeconds = 1f;
    }

    /// <summary>
    /// Applies a 'stunned' state to the target for stunSeconds on each tick.
    /// OneShot -> one stun. Repeating -> several overlapping stun windows.
    /// Repetition from new hits is gated by StackType.None in the resolver.
    /// </summary>
    public sealed class StunResult : IEffectResult
    {
        private readonly float _stunSeconds;

        public StunResult(AppliedStunEffect cfg) => _stunSeconds = Mathf.Max(0f, cfg.stunSeconds);

        public void Apply(EffectContext ctx)
        {
            if (ctx.Target == null || !ctx.TargetReceiver.TryGetStatusReceiver(out var c)) return;
            c.ApplyStun(ctx.Instance.Key, _stunSeconds);
        }

        public void OnExpire(EffectContext ctx)
        {
            // The stun state has its own timer in the status component; effect end
            // does not force-clear it (lets the last stun window finish naturally).
            // If you want effect-end to cancel stun immediately, call rec.ClearStun(key) here.
        }
    }
}