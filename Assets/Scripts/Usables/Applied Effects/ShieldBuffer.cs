using Arcatech.Stats;
using UnityEngine;

namespace Arcatech.Usables.Effects
{
    /// <summary>
    /// A damage-absorbing buffer on a single stat. Has its OWN lifetime (separate
    /// from the effect that spawned it, per design doc). Absorbs negative deltas
    /// up to its coefficient and remaining value, capped by an absorb limit.
    /// </summary>
    public sealed class ShieldBuffer
    {
        public EffectKey Key { get; }
        public ResourceStatType Stat { get; }
        public float Coefficient { get; }
        public float AbsorbLimit { get; }      // max value the buffer may ever hold within the effect
        public float Current { get; private set; }

        private float _lifeRemaining;

        public bool IsExpired => _lifeRemaining <= 0f || Current <= 0f;

        public ShieldBuffer(EffectKey key, ResourceStatType stat, float coefficient,
            float absorbLimit, float bufferLifetime)
        {
            Key = key;
            Stat = stat;
            Coefficient = Mathf.Clamp01(coefficient);
            AbsorbLimit = Mathf.Max(0f, absorbLimit);
            _lifeRemaining = Mathf.Max(0f, bufferLifetime);
            Current = 0f;
        }

        /// <summary> Tick top-up from a shield tick, clamped to the absorb limit. </summary>
        public void TopUp(float amount, float bufferLifetime)
        {
            Current = Mathf.Min(AbsorbLimit, Current + Mathf.Max(0f, amount));
            // refreshing the buffer's own lifetime on top-up (design doc: resets existence time)
            _lifeRemaining = Mathf.Max(_lifeRemaining, Mathf.Max(0f, bufferLifetime));
        }

        public void Tick(float dt) => _lifeRemaining -= dt;

        /// <summary>
        /// Absorbs part of incoming damage. 'damage' is positive magnitude.
        /// Returns how much got through (unabsorbed).
        /// </summary>
        public float Absorb(float damage)
        {
            if (damage <= 0f || Current <= 0f) return damage;

            float potential = Coefficient * damage;       // c * D
            float absorbed = Mathf.Min(potential, Current); // min(c*D, B)
            Current -= absorbed;
            return damage - absorbed;                      // D - absorbed
        }
    }
}