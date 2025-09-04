using Arcatech.Stats;
using System.Collections.Generic;

namespace Arcatech.Items
{
    public interface IEquippable : IItem
    {
        public BaseItemComponent DisplayItem { get; }
        public List<StatsMod> StatMods { get; }
    
    }

    public interface IItem
    {
        public EquipmentType Type { get; }
    }
}