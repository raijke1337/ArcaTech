using Arcatech.Texts;
using System;
using Arcatech.UI;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Items
{
    [Serializable, CreateAssetMenu(fileName = "New Backpack Item", menuName = "Items/Just Item")]
    public class ItemSO : ScriptableObjectID, IIconContent
    {
        [SerializeField] public BaseItemComponent itemPrefab;
        public ExtendedText Description;        
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

        public Sprite Icon => Description.Picture;
        public float FillValue => 0;
        public string IconValue => string.Empty;
    }
}