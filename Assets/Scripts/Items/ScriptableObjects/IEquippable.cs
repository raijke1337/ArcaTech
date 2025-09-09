using Arcatech.Stats;
using System.Collections.Generic;
using Arcatech.UI;

namespace Arcatech.Items
{
    public interface IEquippable : IItem
    {
        public BaseItemComponent DisplayItem { get; }
        public List<StatsMod> StatMods { get; }
        public void OnEquip();
        public void OnUnequip();
    
    }

    public interface IItem
    {
        public SerializableGuid ID { get; }
        public ItemType Type { get; }
    }
}