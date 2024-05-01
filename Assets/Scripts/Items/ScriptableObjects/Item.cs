using Arcatech.Texts;
using System;
using UnityEngine;

namespace Arcatech.Items
{
    [Serializable, CreateAssetMenu(fileName = "New Backpack Item", menuName = "Items/Backpack Item")]
    public class Item : ScriptableObjectID
    {
        public ExtendedTextContainerSO Description;
        public EquipItemType ItemType;
    }
}