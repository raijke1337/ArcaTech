namespace Arcatech.Units.Control
{
    /// <summary>
    /// this interface will be checking is a command can be done
    /// </summary>
    public interface IUnitCommandValidator
    {
        public bool CanDoUnitCommand(UnitCommand command, out string info);
    }
}