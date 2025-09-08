using System;
using Arcatech.UI;
using UnityEditor;
using UnityEngine;

namespace Arcatech.Items
{
    // coin, upgrade, key etc..
    [Serializable]
    public class Item : IItem, IIconContent
    {
        public BaseGameEntityComponent Owner { get; }
        public SerializableGuid ID;
        public ItemSO Config;
        public Item(ItemSO cfg, BaseGameEntityComponent ow)
        {
            ID = cfg.ID;
            Owner = ow;
            Config = cfg;
            Type = cfg.type;
        }
        
        public ItemType Type { get; protected set; }
        public virtual Sprite Icon =>  Config.Description.Picture;
        public virtual float FillValue => 0;
        public virtual string IconValue => string.Empty;

    }
}