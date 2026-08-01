using System;
using Arcatech.Texts;
using Arcatech.UI;
using UnityEngine;

namespace Arcatech.Items
{
    [Serializable, CreateAssetMenu(fileName = "item_", menuName = "Items/Just Item")]
    public class ItemSO : ScriptableObjectID, IHasDescription
    {
        
        [SerializeField] Description description;        
        [Space] public int MaxStack = 1;
        [SerializeField] public ItemPickUpEffect worldItemContainerPrefab;

        public virtual Item BuildItem(BaseGameEntityComponent owner)
        {
            return new Item(this,owner);
        }

        public Description Description => description;
        public float FillValue => 0;
        public string StringInfo => string.Empty;

    }
}