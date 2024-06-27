using Arcatech.Texts;
using Arcatech.Units;
using System;
using UnityEngine;

namespace Arcatech.Items
{
    // coin, upgrade, key etc..
    [Serializable]
    public class InventoryItem
    {
        public string ID;
        public Sprite ItemIcon { get => _description.Picture; }
        public ExtendedText GetDescription => _description;
        public EquipmentType ItemType { get; }

        protected ExtendedText _description;
        protected Item _config;

        public InventoryItem(Item config)
        {
            ID = config.ID; ItemType = config.ItemType;
            _description = (config.Description);
            _config = config; // use in child classes
        }

    }
}