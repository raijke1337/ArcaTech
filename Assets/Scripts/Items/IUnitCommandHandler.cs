
using Arcatech.Stats;
using Arcatech.Units;

namespace Arcatech.Items
{
    public interface IUnitCommandHandler
    {
        bool TryHandleUnitCommand(UnitActionType type, EntityStatsComponent stats, out UnitState state);
    }


}