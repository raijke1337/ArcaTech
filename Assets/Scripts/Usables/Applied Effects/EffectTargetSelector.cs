using Arcatech;

namespace ArcaTech.Usables.Effects
{
    public interface ITargetSelector
    {
        bool TryPickTarget(TargetingType targeting,
            BaseGameEntityComponent source,
            BaseGameEntityComponent hit,
            out BaseGameEntityComponent finalTarget);
    }
    public sealed class EffectTargetSelector : ITargetSelector
    {
        public bool TryPickTarget(TargetingType targeting,
            BaseGameEntityComponent source,
            BaseGameEntityComponent hit,
            out BaseGameEntityComponent finalTarget)
        {
            finalTarget = null;

            switch (targeting)
            {
                case TargetingType.None:
                    return false;

                case TargetingType.ApplyToSource:
                    finalTarget = source;
                    break;

                case TargetingType.ApplyToEnemyTarget:
                    if (hit == null || hit == source) return false;
                    if (source.GetEntitySide == Side.Unassigned) return false;
                    if (hit.GetEntitySide == source.GetEntitySide) return false;
                    finalTarget = hit;
                    break;

                case TargetingType.ApplyToAlliedTarget:
                    if (hit == null || hit == source) return false;
                    if (hit.GetEntitySide != source.GetEntitySide) return false;
                    finalTarget = hit;
                    break;

                case TargetingType.ApplyToAnyTargetExceptSource:
                    if (hit == null || hit == source) return false;
                    finalTarget = hit;
                    break;

                case TargetingType.ApplyToAnyTarget:
                    finalTarget = hit;
                    break;

                default:
                    return false;
            }

            return finalTarget != null;
        }
    }
}