using Arcatech.Items;
using Arcatech.Stats;
using Arcatech.UI;
using Arcatech.Units;

namespace Arcatech
{
    public interface IUsable : ICosted, IActionTypeItem, IIconContent
    {
        public string UsableName { get; }
        public bool CanUseItem(EntityStatsComponent stats);
        public bool TryUseItem(EntityStatsComponent stats, out UnitState state);
        void DoUpdate(float delta);
    }

   
}