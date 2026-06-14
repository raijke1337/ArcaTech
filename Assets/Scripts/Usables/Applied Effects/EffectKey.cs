using System;

namespace Arcatech.Usables.Effects
{
    /// <summary>
    /// Identity of an effect instance on a target, per design doc:
    /// effect's serialized string ID (Guid) + source entity ID.
    /// Used for stacking/grouping.
    /// </summary>
    public readonly struct EffectKey : IEquatable<EffectKey>
    {
        public readonly string EffectId;
        public readonly string SourceId;

        public EffectKey(string effectId, string sourceId)
        {
            EffectId = effectId ?? string.Empty;
            SourceId = sourceId ?? string.Empty;
        }

        public bool Equals(EffectKey other) =>
            string.Equals(EffectId, other.EffectId, StringComparison.Ordinal) &&
            string.Equals(SourceId, other.SourceId, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is EffectKey o && Equals(o);

        public override int GetHashCode()
        {
            unchecked
            {
                return (StringComparer.Ordinal.GetHashCode(EffectId) * 397)
                       ^ StringComparer.Ordinal.GetHashCode(SourceId);
            }
        }

        public override string ToString() => $"eff#{EffectId}/src#{SourceId}";
    }
}