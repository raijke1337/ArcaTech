using Arcatech.Stats;
using Arcatech.Triggers;
using System.Collections.Generic;

namespace Arcatech.Items
{
    public interface IEquippable : IHasSkill, IItem
    {
        public BaseItemComponent DisplayItem { get; }
        public List<StatsMod> StatMods { get; }
    
    }

    public interface IItem
    {
        public EquipmentType Type { get; }
    }
}