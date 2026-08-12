namespace Arcatech.Units.Control
{
    /// <summary>
    /// this interface just does the command, no checks
    /// </summary>
    public interface IUnitCommandPerformer
    {
        void PrepareCommand(UnitCommand command);
        void DoUnitCommand(UnitCommand command,bool wasSuccessful);
    }


}