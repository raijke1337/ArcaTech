using Arcatech.Items;
using System.Collections.Generic;
namespace Arcatech.Managers
{
    public class GameSaveData
    {
        public List<string> OpenedLevelsID;
        public UnitInventoryContainer Inventory { get; protected set; }
        public void UpdateInventory(UnitInventoryContainer c) 
        {
            Inventory = c;
        }
    }
}