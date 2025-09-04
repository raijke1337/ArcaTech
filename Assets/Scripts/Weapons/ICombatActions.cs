using Arcatech.Units;

namespace Arcatech
{
    /// <summary>
    /// for controller components
    /// </summary>
    public interface ICombatActions : IManagedController
    {
        public bool ActionAvailable(UnitActionType action);
        public bool CanUseAction(UnitActionType action);
        public bool TryUseAction(UnitActionType action, out BaseUnitAction onUse);
    }
}