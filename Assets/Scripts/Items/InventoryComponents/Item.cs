using System;
using Arcatech.Texts;
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
        public SerializableGuid ID => Config.ID;
        public ItemSO Config { get; }
        public Item(ItemSO cfg, BaseGameEntityComponent ow)
        {
            Owner = ow;
            Config = cfg;
            Type = cfg.type;
        }
        
        public ItemType Type { get; protected set; }
        public virtual Description Description =>  Config.Description;
        public virtual float FillValue => 0;
        public virtual string IconNumber => string.Empty;

    }
}