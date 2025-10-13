using Arcatech.Units;

namespace Arcatech
{
    public interface IUsablesStrategy : IStrategy
    {
        ActiveGameUnitComponent Owner { get; }
        bool CanUseUsable();
        bool TryUseUsable(out UnitState state);
        void UpdateUsable(float delta);
    }
}