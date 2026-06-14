using Arcatech.Stats;
using Arcatech.Usables.Effects;

namespace Arcatech
{
    public interface ICosted
    {
        public AppliedStatsDeltaEffect GetCost { get; }
    }
}