using System;
using Arcatech.Managers;
using Arcatech.Texts;
using Arcatech.UI;
using UnityEditor;
using UnityEngine;

namespace Arcatech.Items
{
    // coin, upgrade, key etc..
    [Serializable]
    public class Item : IIconContent
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
        public virtual float FillValue => 0;
        public virtual string StringInfo => string.Empty;

        public ItemPickUpEffect PackItem
        {
            get
            {
                var box = GameObject.Instantiate(Config.worldItemContainerPrefab);
                box.PutItem(Config);
                return box;
            }
        }
    }
}