using Arcatech.Stat;
using Arcatech.Units;

namespace Arcatech.Items
{
    public interface IUnitActionsHandler
    {
        bool TryHandleAction(UnitActionType type, EntityStatsComponent stats, out BaseUnitAction action);
    }


}