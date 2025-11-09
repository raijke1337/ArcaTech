using Arcatech.Stats;
using Arcatech.Triggers;

namespace Arcatech
{
    public interface ICosted
    {
        public StatsEffect GetCost { get; }
    }

   
}