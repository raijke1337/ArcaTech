using System;
using Arcatech.Texts;
using Arcatech.UI;
using UnityEngine;

namespace Arcatech.Items
{
    // coin, upgrade, key etc..
    [Serializable]
    public class Item : IHasDescription
    {
        protected ItemSO Config;
        public BaseGameEntityComponent Owner { get; }
        public string ID { get; }
        public Item(ItemSO cfg, BaseGameEntityComponent ow)
        {
            Owner = ow;
            Description = cfg.Description;
            ID =  cfg.ID;
            Config = cfg;
        }
        public ItemSlot Slot { get; protected set; }
        public virtual Description Description { get; }

    }
}