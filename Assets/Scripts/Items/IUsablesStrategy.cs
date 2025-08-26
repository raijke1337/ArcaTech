using Arcatech.Units;

namespace Arcatech
{
    public interface IUsablesStrategy : IStrategy
    {
        BaseEntityOLD Owner { get; }
        bool CanUseUsable();
        bool TryUseUsable(out BaseUnitAction action);
        void UpdateUsable(float delta);
    }
}