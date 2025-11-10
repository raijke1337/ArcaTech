using Arcatech.Units;

namespace Arcatech
{
    public interface IUsableStrategy : IStrategy
    {
        EntityStateMachineComponent Owner { get; }
        bool CanUseUsable();
        UnitState UseUsable();
        void UpdateUsable(float delta);
    }
}