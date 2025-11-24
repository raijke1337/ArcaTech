using System.Collections.Generic;

namespace Arcatech.Items
{
    public interface IDrawItemStrategy : IStrategy 
    {
        public Dictionary<ItemSlot, ItemPlaceType> GetPlaces { get; }
    }


}