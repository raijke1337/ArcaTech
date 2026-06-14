using NUnit.Framework;
using Arcatech.Usables.Effects;
using StackType = Arcatech.Usables.Effects.BaseAppliedEffect.StackType;

namespace Arcatech.Tests.EditMode
{
    public class StackingResolverTests
    {
        private readonly StackingResolver _r = new StackingResolver();

        // a tiny stand-in list; resolver only reads Count, so empty instances are fine
        private static System.Collections.Generic.List<ActiveEffectInstance> WithCount(int n)
        {
            var list = new System.Collections.Generic.List<ActiveEffectInstance>();
            for (int i = 0; i < n; i++) list.Add(null); // resolver only uses Count
            return list;
        }

        // ---- None (stun) ----

        [Test]
        public void None_FirstApplication_Adds()
        {
            Assert.AreEqual(StackDecision.Add, _r.Resolve(WithCount(0), StackType.None, 99));
        }

        [Test]
        public void None_WhileActive_Rejects()
        {
            Assert.AreEqual(StackDecision.Reject, _r.Resolve(WithCount(1), StackType.None, 99));
        }

        // ---- Refresh (shield) ----

        [Test]
        public void Refresh_FirstApplication_Adds()
        {
            Assert.AreEqual(StackDecision.Add, _r.Resolve(WithCount(0), StackType.Refresh, 99));
        }

        [Test]
        public void Refresh_WhenExisting_Refreshes()
        {
            Assert.AreEqual(StackDecision.Refresh, _r.Resolve(WithCount(1), StackType.Refresh, 99));
        }

        // ---- Independent (stat change) ----

        [Test]
        public void Independent_BelowCap_Adds()
        {
            Assert.AreEqual(StackDecision.Add, _r.Resolve(WithCount(2), StackType.Independent, 3));
        }

        [Test]
        public void Independent_AtCap_Rejects()
        {
            Assert.AreEqual(StackDecision.Reject, _r.Resolve(WithCount(3), StackType.Independent, 3));
        }

        [Test]
        public void Independent_CapClampedToAtLeastOne()
        {
            // maxStacks <= 0 must still allow the first
            Assert.AreEqual(StackDecision.Add, _r.Resolve(WithCount(0), StackType.Independent, 0));
            Assert.AreEqual(StackDecision.Reject, _r.Resolve(WithCount(1), StackType.Independent, 0));
        }
    }
}