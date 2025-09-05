using System.Collections.Generic;

namespace Arcatech.Items
{
    public interface IDrawItemStrategy : IStrategy 
    {
        public Dictionary<EquipmentType, ItemPlaceType> GetPlaces { get; }
    }


}