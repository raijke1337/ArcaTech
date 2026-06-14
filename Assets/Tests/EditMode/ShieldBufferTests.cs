using NUnit.Framework;
using Arcatech.Usables.Effects;
using Arcatech.Stats;

namespace Arcatech.Tests.EditMode
{
    public class ShieldBufferTests
    {
        private static EffectKey K => new EffectKey("shield", "src");

        [Test]
        public void DesignDocExample_TwoHits_AbsorbAndOverflow()
        {
            // shield: 100/tick, coeff 0.6, no limit issue here (limit high), buffer life 5
            var buf = new ShieldBuffer(K, ResourceStatType.Health,
                coefficient: 0.6f, absorbLimit: 1000f, bufferLifetime: 5f);

            buf.TopUp(100f, 5f);                 // tick 1: buffer = 100
            Assert.AreEqual(100f, buf.Current, 1e-3f);

            // hit -80: potential 48, absorbed 48, through = 80-48 = 32
            float through1 = buf.Absorb(80f);
            Assert.AreEqual(32f, through1, 1e-3f, "first hit damage through");
            Assert.AreEqual(52f, buf.Current, 1e-3f, "buffer left after first hit");

            // hit -100: potential 60, buffer only 52 -> absorbed 52, overflow 8
            // base through (1-c)*100 = 40, plus overflow 8 = 48
            float through2 = buf.Absorb(100f);
            Assert.AreEqual(48f, through2, 1e-3f, "second hit damage through (40 + 8 overflow)");
            Assert.AreEqual(0f, buf.Current, 1e-3f, "buffer emptied");
        }

        [Test]
        public void AbsorbLimit_CapsAccumulation()
        {
            // 3 ticks of 100 but limit 150 -> 100 -> 150 -> 150
            var buf = new ShieldBuffer(K, ResourceStatType.Health, 0.6f, absorbLimit: 150f, bufferLifetime: 5f);
            buf.TopUp(100f, 5f); Assert.AreEqual(100f, buf.Current, 1e-3f);
            buf.TopUp(100f, 5f); Assert.AreEqual(150f, buf.Current, 1e-3f);
            buf.TopUp(100f, 5f); Assert.AreEqual(150f, buf.Current, 1e-3f);
        }

        [Test]
        public void NoBuffer_DamagePassesThroughUnchanged()
        {
            var buf = new ShieldBuffer(K, ResourceStatType.Health, 0.6f, 150f, 5f);
            // never topped up -> Current 0 -> nothing absorbed
            Assert.AreEqual(80f, buf.Absorb(80f), 1e-3f);
        }

        [Test]
        public void Expires_ByOwnTimer()
        {
            var buf = new ShieldBuffer(K, ResourceStatType.Health, 0.6f, 150f, bufferLifetime: 2f);
            buf.TopUp(100f, 2f);
            buf.Tick(1f); Assert.IsFalse(buf.IsExpired);
            buf.Tick(1.1f); Assert.IsTrue(buf.IsExpired, "buffer expires by its own lifetime");
        }
    }
}