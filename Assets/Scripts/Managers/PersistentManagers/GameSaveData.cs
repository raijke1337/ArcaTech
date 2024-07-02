﻿using Arcatech.Items;
using Arcatech.Managers.Save;
using System.Collections.Generic;
namespace Arcatech.Managers
{
    public class GameSaveData
    {
        public List<string> OpenedLevelsID;
        public UnitInventoryItemConfigsContainer Inventory { get; protected set; }
        public void UpdateInventory(UnitInventoryItemConfigsContainer c) 
        {
            Inventory = c;
        }
    }
}