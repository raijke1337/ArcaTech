using Arcatech.Items;
using UnityEngine;

namespace Arcatech.Usables.Effects
{
    public enum SuitDamageOperation
    {
        Damage,     // +steps ступеней повреждения
        Repair,     // -steps ступеней (ремонтная станция)
        FullRepair  // сразу «Целый»
    }

    /// <summary>
    /// Урон/ремонт костюма цели. Цель должна иметь SuitDamageComponent.
    /// Вешается на атаки врагов, ловушки, QTE-захваты и ремонтные станции как любой другой эффект
    /// (SerializedApplyUsableEffectsResult). Для ремонтной станции используйте TargetingType.ApplyToSource/AnyTarget.
    /// </summary>
    [CreateAssetMenu(fileName = "usableEffect_suit_", menuName = "Usables/Applied Effects/Suit damage")]
    public class AppliedSuitDamageEffect : BaseAppliedEffect
    {
        [Header("Suit damage")]
        public SuitDamageOperation operation = SuitDamageOperation.Damage;
        [Min(1), Tooltip("Количество ступеней для Damage/Repair. Для FullRepair игнорируется.")]
        public int steps = 1;

        [Tooltip("Урон по костюму не проходит, пока цель неуязвима (например, во время переката).")]
        public bool respectInvulnerability = true;

        // Мгновенный эффект: без длительности и со стаком Independent, иначе стандартные 3 секунды
        // и StackType.None отклоняли бы повторные попадания, пока предыдущий экземпляр «жив».
        private void Reset()
        {
            durationSeconds = 0f;
            stackType = StackType.Independent;
        }
    }

    public sealed class SuitDamageResult : BaseResult
    {
        private readonly SuitDamageOperation _operation;
        private readonly int _steps;
        private readonly bool _respectInvulnerability;

        public SuitDamageResult(AppliedSuitDamageEffect cfg) : base(cfg)
        {
            _operation = cfg.operation;
            _steps = Mathf.Max(1, cfg.steps);
            _respectInvulnerability = cfg.respectInvulnerability;
        }

        public override bool Validate(EffectContext ctx) =>
            ctx.Target != null && ctx.Target.TryGetComponent<SuitDamageComponent>(out _);

        public override void Apply(EffectContext ctx)
        {
            if (ctx.Target == null || !ctx.Target.TryGetComponent<SuitDamageComponent>(out var suit)) return;

            switch (_operation)
            {
                case SuitDamageOperation.Damage:
                    if (_respectInvulnerability && ctx.Target.Invulnerable) return;
                    suit.Damage(_steps);
                    break;
                case SuitDamageOperation.Repair:
                    suit.Repair(_steps);
                    break;
                case SuitDamageOperation.FullRepair:
                    suit.FullRepair();
                    break;
            }
        }

        public override void OnExpire(EffectContext ctx)
        {
            // Повреждение костюма постоянно и снимается только ремонтом, откатывать нечего.
        }
    }
}
