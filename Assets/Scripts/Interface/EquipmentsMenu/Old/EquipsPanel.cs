
using Arcatech.Items;
using AYellowpaper.SerializedCollections;
using System;
using UnityEngine;
namespace Arcatech.UI
{
    public class EquipsPanel : InventoryItemsHolder
    {
        [SerializeField] protected SerializedDictionary<ItemType, ItemTileComponent> EquipsTiles;
        public override ItemTileComponent AddTileContent(Item content)
        {
            if (content == null) return null;
            try
            {
                var tile = EquipsTiles[content.Type];
                tile.Item = content;
                return tile;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"No slot for item in {this} {e}");
                return null;
            }

        }

        public override ItemTileComponent RemoveTileContent(Item content)
        {
            if (content == null) return null;
            try
            {
                var t = EquipsTiles[content.Type];
                t.Clear();
                return t;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"No equipped item {content} {e}");
                return null;
            }
        }

    }

}