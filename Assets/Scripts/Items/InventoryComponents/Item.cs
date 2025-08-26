using Arcatech.Texts;
using Arcatech.Units;
using System;
using UnityEngine;

namespace Arcatech.Items
{
    // coin, upgrade, key etc..
    [Serializable]
    public class Item : IItem
    {
        public ActiveGameUnitComponent Owner { get; }
        public SerializableGuid ID;
        public ItemSO Config;
        public Item(ItemSO cfg, ActiveGameUnitComponent ow)
        {
            ID = cfg.ID;
            Owner = ow;
            Config = cfg;
            Type = cfg.Type;
        }


        public EquipmentType Type { get; protected set; }
    }
}