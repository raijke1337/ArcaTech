using Arcatech.Units.Combat;
using UnityEngine;

namespace Arcatech.Usables.Effects
{
    [CreateAssetMenu(fileName = "usableEffect_summon_", menuName = "Usables/Applied Effects/Summon")]
    public class AppliedSummonEffect : BaseAppliedEffect
    {
        [Header("Summon")]
        public BaseGameEntityComponent[] prefabs;
        [Tooltip("Spawn offsets relative to the target, paired by index with prefabs. " +
                 "Missing entries default to Vector3.zero.")]
        public Vector3[] offsets;

        [Header("Combat")]
        [Tooltip("Агрессия призванных на спавне. 1 = полная шкала: сразу претендуют на токен атаки. " +
                 "Призванные также берут боевую группу призывателя и сразу вступают в бой.")]
        [Range(0f, CombatParticipant.MaxAggression)]
        public float spawnAggression = 1.2f;
    }

    /// <summary>
    /// Instantiates prefabs at offsets relative to ctx.Target.
    /// Target identity comes from the TargetSelector: ApplyToSource -> spawns on the
    /// caster, ApplyToEnemyTarget -> spawns on the hit enemy, etc.
    /// Each tick spawns a wave (repeating -> multiple waves), per design doc.
    /// Summoned entities live by their own lifetime, not tied to this effect.
    /// </summary>
    public sealed class SummonResult : BaseResult
    {
        private readonly BaseGameEntityComponent[] _prefabs;
        private readonly Vector3[] _offsets;
        private readonly float _spawnAggression;

        public SummonResult(AppliedSummonEffect cfg): base(cfg)
        {
            _prefabs = cfg.prefabs ?? System.Array.Empty<BaseGameEntityComponent>();
            _offsets = cfg.offsets ?? System.Array.Empty<Vector3>();
            _spawnAggression = cfg.spawnAggression;
        }

        public override void Apply(EffectContext ctx)
        {
            if (ctx.Target == null) return;

            var origin = ctx.Target.transform;

            for (int i = 0; i < _prefabs.Length; i++)
            {
                if (_prefabs[i] == null) continue;

                Vector3 offset = i < _offsets.Length ? _offsets[i] : Vector3.zero;
                // offset interpreted in the target's local space so it rotates with facing
                Vector3 pos = origin.position + origin.TransformDirection(offset);
                Quaternion rot = origin.rotation;

                var spawned = GameObject.Instantiate(_prefabs[i], pos, rot);
                if (spawned.TryGetComponent(out CombatParticipant participant))
                    participant.OnSummoned(_spawnAggression, ctx.Source);
            }
        }
        public override bool Validate(EffectContext ctx) => true;
        public override void OnExpire(EffectContext ctx) { }
    }
}