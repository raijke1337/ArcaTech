using Arcatech.Stats;

namespace Arcatech
{
    public interface ICosted
    {
        public AppliedStatsDeltaEffect GetCost { get; }
    }
}