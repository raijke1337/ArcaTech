using System;
using Arcatech.Items;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Arcatech.UI
{
    public class InventoryItemsHolder : MonoBehaviour
    {
        [SerializeField] protected ItemTileComponent TilePrefab;
        [SerializeField] protected Transform _iconsParent;
        protected List<ItemTileComponent> _tiles;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <returns>tile where the item is placed</returns>
        public virtual ItemTileComponent AddTileContent(Item content)
        {
            if (content == null) return null;
            if (_tiles == null) _tiles = new List<ItemTileComponent>();

            var tile = Instantiate(TilePrefab, _iconsParent);
            _tiles.Add(tile);

            tile.Item = content;

            tile.enabled = true;

            return tile;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <returns>tile that was destroyed</returns>
        public virtual ItemTileComponent RemoveTileContent(Item content)
        {
            if (content == null) return null;
            var tile = _tiles.First(t => t.Item == content);
            _tiles.Remove(tile);
            Destroy(tile.gameObject);
            return tile;
        }

        private void Start()
        {
            _tiles = new List<ItemTileComponent>();
        }
    }
}