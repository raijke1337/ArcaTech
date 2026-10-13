using System;
using Arcatech.Texts;
using Arcatech.UI;
using UnityEngine;

namespace Arcatech.Items
{
    /// <summary>Как предмет учитывается системой сохранений.
    /// Экипировка (EquipSO) определяется по типу и этого поля не использует.</summary>
    public enum ItemCategory
    {
        /// <summary>Нужен только на уровне (ключи и т.п.), в мета-прогресс не попадает.</summary>
        LevelOnly,
        /// <summary>Деталь - валюта Мастерской.</summary>
        Part,
        CostumeBlueprint,
        GalleryBlueprint
    }

    [Serializable, CreateAssetMenu(fileName = "item_", menuName = "Items/Just Item")]
    public class ItemSO : ScriptableObjectID, IHasDescription
    {
        
        [SerializeField] Description description;        
        [Space] public int MaxStack = 1;
        [SerializeField] public ItemPickUpEffect worldItemContainerPrefab;
        [Tooltip("Как предмет переносится в мета-прогресс при прохождении уровня")]
        public ItemCategory Category = ItemCategory.LevelOnly;

        public virtual Item BuildItem(BaseGameEntityComponent owner)
        {
            return new Item(this,owner);
        }

        public Description Description => description;
        public float FillValue => 0;
        public string StringInfo => string.Empty;

    }
}