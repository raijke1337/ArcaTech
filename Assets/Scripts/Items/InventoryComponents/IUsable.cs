using Arcatech.Items;
using Arcatech.Stats;
using Arcatech.UI;
using Arcatech.Units;

namespace Arcatech
{
    public interface IUsable : ICosted, IActionTypeItem, IIconContent
    {
        public string UsableName { get; }
        public bool CanUseItem(UnitStatsControllerOLD stats);
        public bool TryUseItem(UnitStatsControllerOLD stats, out BaseUnitAction action);
        void DoUpdate(float delta);
    }

   
}