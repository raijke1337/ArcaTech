using Arcatech.Texts;
using System;
using System.Security.Cryptography;
using Arcatech.UI;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Items
{
    [Serializable, CreateAssetMenu(fileName = "New Backpack Item", menuName = "Items/Just Item")]
    public class ItemSO : ScriptableObjectID, IIconContent
    {
        
        [SerializeField] Description description;        
        [Space] public int MaxStack = 1;
        [SerializeField] private ItemContainerComponent worldItemContainer;

        public virtual Item BuildItem(BaseGameEntityComponent owner)
        {
            return new Item(this,owner);
        }

        public Description Description => description;
        public float FillValue => 0;
        public string IconNumber => string.Empty;

        public ItemContainerComponent PackItem()
        {
            var box = Instantiate(worldItemContainer);
            box.PutItem(this);
            return box;
        }
    }
}