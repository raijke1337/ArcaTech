using System.Collections.Generic;

namespace Arcatech.Items
{
    public interface IEntityItemsList
    {
        public List<Item> GetEquipment(BaseGameEntityComponent owner);
        public Dictionary<Item,int> GetInventory(BaseGameEntityComponent owner);
    }
}