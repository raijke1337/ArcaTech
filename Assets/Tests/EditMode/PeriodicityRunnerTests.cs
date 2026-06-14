using NUnit.Framework;
using Arcatech.Usables.Effects;

namespace Arcatech.Tests.EditMode
{
    public class PeriodicityRunnerTests
    {
        // ---------- helpers ----------

        /// <summary>
        /// Drives the runner with a single big "elapsed" value and counts how many
        /// ticks mature up to that point. Mirrors how ActiveEffectInstance consumes ticks.
        /// </summary>
        private static int ConsumeAt(PeriodicityRunner r, float elapsed)
        {
            int count = 0;
            while (r.TryConsumeTick(elapsed)) count++;
            return count;
        }

        private static PeriodicityDefinition OneShot(OneShotMoment moment) => new()
        {
            kind = PeriodicityKind.OneShot,
            oneShotMoment = moment
        };

        private static PeriodicityDefinition Repeating(int ticks, IntervalMode mode, float offset) => new()
        {
            kind = PeriodicityKind.Repeating,
            ticks = ticks,
            intervalMode = mode,
            offsetSeconds = offset
        };

        // ---------- OneShot ----------

        [Test]
        public void OneShot_AtStart_FiresImmediately()
        {
            var r = new PeriodicityRunner(OneShot(OneShotMoment.AtStart), totalDuration: 5f);

            Assert.AreEqual(1, ConsumeAt(r, 0f), "AtStart must fire at elapsed 0");
            Assert.IsTrue(r.AllTicksConsumed);
            Assert.AreEqual(0, ConsumeAt(r, 5f), "no further ticks after the single one");
        }

        [Test]
        public void OneShot_AtEnd_FiresOnlyAtTotalDuration()
        {
            var r = new PeriodicityRunner(OneShot(OneShotMoment.AtEnd), totalDuration: 5f);

            Assert.AreEqual(0, ConsumeAt(r, 4.99f), "must not fire before total duration");
            Assert.AreEqual(1, ConsumeAt(r, 5f), "fires exactly at total duration");
            Assert.IsTrue(r.AllTicksConsumed);
        }

        // ---------- Repeating: the shield case from the design doc ----------

        [Test]
        public void Repeating_Before_ShieldCase_TicksAtZeroAndHalf()
        {
            // Design doc shield example: total=5, ticks=2, offset=0, Before
            // Interval = (5 - 0) / 2 = 2.5  -> ticks at 0.0 and 2.5
            var r = new PeriodicityRunner(Repeating(2, IntervalMode.Before, 0f), totalDuration: 5f);

            Assert.AreEqual(1, ConsumeAt(r, 0f),   "first tick at t=0");
            Assert.AreEqual(0, ConsumeAt(r, 2.49f),"second tick not yet at 2.49");
            Assert.AreEqual(1, ConsumeAt(r, 2.5f), "second tick at t=2.5");
            Assert.IsTrue(r.AllTicksConsumed);
        }

        [Test]
        public void Repeating_After_TicksAtIntervalEnds()
        {
            // total=6, ticks=3, offset=0, After
            // Interval = 6/3 = 2 -> ticks at 2, 4, 6
            var r = new PeriodicityRunner(Repeating(3, IntervalMode.After, 0f), totalDuration: 6f);

            Assert.AreEqual(0, ConsumeAt(r, 1.99f), "no tick before first interval ends");
            Assert.AreEqual(1, ConsumeAt(r, 2f),    "tick at 2");
            Assert.AreEqual(1, ConsumeAt(r, 4f),    "tick at 4");
            Assert.AreEqual(1, ConsumeAt(r, 6f),    "tick at 6");
            Assert.IsTrue(r.AllTicksConsumed);
        }

        [Test]
        public void Repeating_Before_WithOffset()
        {
            // total=4, ticks=2, offset=1, Before
            // span = 4-1 = 3, interval = 3/2 = 1.5 -> ticks at 1.0 and 2.5
            var r = new PeriodicityRunner(Repeating(2, IntervalMode.Before, 1f), totalDuration: 4f);

            Assert.AreEqual(0, ConsumeAt(r, 0.99f), "offset not elapsed yet");
            Assert.AreEqual(1, ConsumeAt(r, 1f),    "first tick after offset");
            Assert.AreEqual(1, ConsumeAt(r, 2.5f),  "second tick");
            Assert.IsTrue(r.AllTicksConsumed);
        }

        // ---------- robustness ----------

        [Test]
        public void LagSpike_ConsumesMultipleTicksInOneCall()
        {
            // total=6, ticks=3, After -> ticks at 2,4,6
            var r = new PeriodicityRunner(Repeating(3, IntervalMode.After, 0f), totalDuration: 6f);

            // a single huge frame at elapsed=10 should release all three matured ticks
            Assert.AreEqual(3, ConsumeAt(r, 10f), "all matured ticks released in one frame");
            Assert.IsTrue(r.AllTicksConsumed);
        }

        [Test]
        public void OffsetGreaterThanDuration_ClampsAllTicksToEnd_NoneLost()
        {
            // offset=10 > total=4 -> clamped, all ticks pinned to total, none dropped
            var r = new PeriodicityRunner(Repeating(2, IntervalMode.Before, 10f), totalDuration: 4f);

            Assert.AreEqual(2, r.TotalTicks, "tick count preserved despite bad offset");
            Assert.AreEqual(2, ConsumeAt(r, 4f), "all ticks fire by end, nothing lost");
        }

        [Test]
        public void ZeroTicks_ClampedToOne()
        {
            // design doc: ticks must be > 0; runner clamps to 1
            var r = new PeriodicityRunner(Repeating(0, IntervalMode.After, 0f), totalDuration: 5f);

            Assert.AreEqual(1, r.TotalTicks);
        }

        [Test]
        public void Reset_ReplaysAllTicks()
        {
            // refresh behavior (StackType.Refresh) must re-arm the schedule
            var r = new PeriodicityRunner(Repeating(2, IntervalMode.After, 0f), totalDuration: 4f);

            Assert.AreEqual(2, ConsumeAt(r, 4f));
            Assert.IsTrue(r.AllTicksConsumed);

            r.Reset();
            Assert.IsFalse(r.AllTicksConsumed, "reset re-arms the runner");
            Assert.AreEqual(2, ConsumeAt(r, 4f), "all ticks replay after reset");
        }
    }
}