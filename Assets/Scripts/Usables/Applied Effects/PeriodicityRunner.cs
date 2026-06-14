using UnityEngine;

namespace Arcatech.Usables.Effects
{
    /// <summary>
    /// Owns the timing of an effect instance: when ticks fire over its lifetime.
    /// Pure C#, no UnityEngine.Time — driven by elapsed time passed in by the controller.
    /// Pre-computes exact tick moments to avoid accumulator drift.
    /// </summary>
    public sealed class PeriodicityRunner
    {
        private readonly float[] _tickTimes; // ascending, exact moments since application
        private int _nextTick;               // index into _tickTimes
        public float TotalDuration { get; }

        /// <summary> Total number of ticks scheduled over the lifetime. </summary>
        public int TotalTicks => _tickTimes.Length;

        public PeriodicityRunner(PeriodicityDefinition def, float totalDuration)
        {
            TotalDuration = Mathf.Max(0f, totalDuration);
            _tickTimes = BuildTickTimes(def, TotalDuration);
            _nextTick = 0;
        }

        private static float[] BuildTickTimes(PeriodicityDefinition def, float total)
        {
            if (def.kind == PeriodicityKind.OneShot)
            {
                float t = def.oneShotMoment == OneShotMoment.AtStart ? 0f : total;
                return new[] { t };
            }

            // Repeating
            int ticks = Mathf.Max(1, def.ticks); // design doc: integer > 0
            float offset = Mathf.Clamp(def.offsetSeconds, 0f, total);
            float span = total - offset;          // window in which ticks happen
            // Interval = (T_total - T_offset) / Ticks
            float interval = ticks > 0 ? span / ticks : span;

            var times = new float[ticks];
            for (int i = 0; i < ticks; i++)
            {
                // Before: tick at START of interval i  -> offset + i*interval
                // After:  tick at END   of interval i  -> offset + (i+1)*interval
                float local = def.intervalMode == IntervalMode.Before
                    ? offset + i * interval
                    : offset + (i + 1) * interval;

                times[i] = Mathf.Min(local, total); // clamp to lifetime
            }
            return times;
        }

        /// <summary>
        /// Returns true if there is a tick whose scheduled time has been reached.
        /// Call in a while-loop: multiple ticks can mature within one frame.
        /// </summary>
        public bool TryConsumeTick(float elapsed)
        {
            if (_nextTick >= _tickTimes.Length) return false;
            if (elapsed + 1e-4f < _tickTimes[_nextTick]) return false;
            _nextTick++;
            return true;
        }

        /// <summary> True once all scheduled ticks have been consumed. </summary>
        public bool AllTicksConsumed => _nextTick >= _tickTimes.Length;

        public void Reset() => _nextTick = 0; // used on duration refresh (Stack: Refresh)
    }
}