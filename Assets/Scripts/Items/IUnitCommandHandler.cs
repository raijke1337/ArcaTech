
using Arcatech.Stats;
using Arcatech.Units;

namespace Arcatech.Items
{
    
    
    /// <summary>
    /// this interface will be checking is a command can be done
    /// </summary>
    public interface IUnitCommandValidator
    {
        public bool CanHandleUnitCommand(UnitActionType type);
    }
    /// <summary>
    /// this interface just does the command, no checks
    /// </summary>
    public interface IUnitCommandHandler
    {
        bool DoUnitCommand(UnitActionType type);
    }


}