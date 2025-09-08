using System.Collections.Generic;

namespace Arcatech.Items
{
    public interface IDrawItemStrategy : IStrategy 
    {
        public Dictionary<ItemType, ItemPlaceType> GetPlaces { get; }
    }


}