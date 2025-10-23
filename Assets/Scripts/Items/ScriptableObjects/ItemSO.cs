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
        [SerializeField] public EquipmentComponent itemPrefab;
        [SerializeField] Description description;        
        public ItemType type;

        [Space] public int MaxStack = 1;
        protected virtual void OnValidate()
        {
            Assert.IsFalse(type==ItemType.None);
        }

        public virtual IItem BuildItem(BaseGameEntityComponent owner)
        {
            return new Item(this,owner);
        }

        public Description Description => description;
        public float FillValue => 0;
        public string IconNumber => string.Empty;
    }
}