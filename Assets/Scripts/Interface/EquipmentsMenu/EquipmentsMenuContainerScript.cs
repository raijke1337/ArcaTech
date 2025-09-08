using System.Collections.Generic;
using UnityEngine;
using Arcatech.Items;
using UnityEngine.Assertions;
using AYellowpaper.SerializedCollections;
using System.Linq;
using TMPro;
using UnityEngine.Events;

namespace Arcatech.UI
{
    public class EquipmentsMenuContainerScript : MonoBehaviour, IUnitInventoryView
    {
        #region display;

        [SerializeField] SerializedDictionary<ItemType, ItemTileComponent> _equipsTiles;
        [SerializeField] List<ItemTileComponent> _inventoryTiles = new List<ItemTileComponent>();

        [Space, SerializeField] private ItemTileComponent _tilePreset; // for items
        [SerializeField] private Transform _tileParent; // for items too

        [SerializeField] ItemCardComponent _itemTooltipPanel;
        [SerializeField] ItemCardComponent _skillTooltipPanel;



        #endregion

        UnitInventoryModel _inventoryModel;



        private void Awake()
        {
            Assert.IsNotNull(_tilePreset);
            Assert.IsNotNull(_tileParent);
            Assert.IsNotNull(_equipsTiles);
            Assert.IsNotNull(_swapItemsButton);


            _itemTooltipPanel.gameObject.SetActive(false);
            _skillTooltipPanel.gameObject.SetActive(false);
            _swapItemsButton.gameObject.SetActive(false);
        }
        public void RefreshView(UnitInventoryModel model)
        {
            Debug.Log($"load {model} into {this}");

            _inventoryModel = model;
            //clean up
            foreach (var tile in _equipsTiles.Values)
            {
                tile.Item = null;
                tile.IconClickedEvent -= IconTileClicked;
            }
            foreach (var tile in _inventoryTiles)
            {
                tile.Item = null;
                tile.IconClickedEvent -= IconTileClicked;
            }

            if (_hldTile != null)
            {
                _hldTile.HighLightToggle();
                _hldTile = null;
            }

            //draw
            foreach (Equipment equip in model.ListEquipped)
            {
                _equipsTiles[equip.Type].Item = equip;
                _equipsTiles[equip.Type].IconClickedEvent += IconTileClicked;
            }
            var inve = model.ListInventory;

            int placedTiles = _inventoryTiles.Count;

            int counter = 0;
            while (counter < placedTiles && counter < inve.Count())
            {
                _inventoryTiles[counter].gameObject.SetActive(true);
                _inventoryTiles[counter].IconClickedEvent += IconTileClicked;
                _inventoryTiles[counter].Item = inve.ToArray()[counter];
                counter++;
            }

            if (counter == placedTiles)
            // case : not enough tiles
            {
                while (counter < inve.Count())
                {
                    var tile = Instantiate(_tilePreset, _tileParent);
                    tile.Item = inve.ToArray()[counter];
                    _inventoryTiles.Add(tile);
                    tile.IconClickedEvent += IconTileClicked;
                    counter++;
                }
            }
            //case : some tiles left : deactivate the empty ones
            if (counter == inve.Count())
            {
                while (counter < _inventoryTiles.Count)
                {
                    _inventoryTiles[counter].gameObject.SetActive(false);
                    counter++;
                }
            }
        }

        #region item operations

        public event UnityAction ViewChangedInventory = delegate { };

        [SerializeField] private TextMeshProUGUI _swapItemsButton;
        private Item _selectedItem;
        public void UI_OnUpdateButtonClick()
        {
            _itemTooltipPanel.gameObject.SetActive(false);
            _skillTooltipPanel.gameObject.SetActive(false);
            _swapItemsButton.gameObject.SetActive(false);

            Debug.LogError("View event NYI");

            //ViewChangedInventory.Invoke(new UnitInventoryViewReference(x,x));
        }

        private IconTileComp _hldTile;
        private void IconTileClicked(IconTileComp arg)
        {
            if (_hldTile != null) { _hldTile.HighLightToggle(); }

            arg.HighLightToggle();
            _hldTile = arg;
            ItemTileClicked(arg.Item);
        }
        private void ItemTileClicked(Item arg)
        {
            _selectedItem = arg;
            ShowTooltips(arg);
            ShowButton(arg);
        }



        private void ShowTooltips(Item item)
        {
            _itemTooltipPanel.gameObject.SetActive(false);
            _skillTooltipPanel.gameObject.SetActive(false);

            if (item == null) return;

            var desc = item.Config.Description;
            /*
            try
            {
                _itemTooltipPanel.gameObject.SetActive(true);
                _itemTooltipPanel.Texts = desc;
            }
            catch { }
            */

            if (item is Equipment e)
            {
                try
                {
                    _skillTooltipPanel.gameObject.SetActive(true);
                   // desc = e.GetSkillData.Descrription;

                   // _skillTooltipPanel.Texts = desc;

                }
                catch { }
            }
        }

        private void ShowButton(Item item)
        {
            //_swapItemsButton.gameObject.SetActive(false);
            //if (item is Equipment eq)
            //{
            //    _swapItemsButton.gameObject.SetActive(true);
            //    if (_inventoryModel.Equipments.TryGetValue(item.Type, out _))
            //    {
            //        _swapItemsButton.SetText("UNEQUIP");
            //    }
            //    else
            //    {
            //        _swapItemsButton.SetText("EQUIP");
            //    }
            //}
        }


        #endregion



    }
}