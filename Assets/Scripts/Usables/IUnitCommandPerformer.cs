using Arcatech.Stats;
using Arcatech.Units;

namespace Arcatech.Items
{
    
    
    /// <summary>
    /// this interface will be checking is a command can be done
    /// </summary>
    public interface IUnitCommandValidator
    {
        public bool CanDoUnitCommand(UnitActionType type, out string info);
    }
    /// <summary>
    /// this interface just does the command, no checks
    /// </summary>
    public interface IUnitCommandPerformer
    {
        void PrepareCommand(UnitActionType type);
        bool DoUnitCommand(UnitActionType type,bool wasSuccessful);
    }


}