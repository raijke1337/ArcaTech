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
        public BaseGameEntityComponent Owner { get; }
        public SerializableGuid ID { get; }
        public Item(ItemSO cfg, BaseGameEntityComponent ow)
        {
            Owner = ow;
            Description = cfg.Description;
            ID =  cfg.ID;
            Boxed = cfg.PackItem();
            Boxed.gameObject.SetActive(false);
        }
        public ItemSlot Slot { get; protected set; }
        public virtual Description Description { get; }
        public virtual float FillValue => 0;
        public virtual string IconNumber => string.Empty;
        public ItemContainerComponent Boxed { get; }

    }
}