using Arcatech.Units;

namespace Arcatech
{
    public interface IUsableStrategy : IStrategy
    {
        ActiveGameUnitComponent Owner { get; }
        bool CanUseUsable();
        UnitState UseUsable();
        void UpdateUsable(float delta);
    }
}