using Arcatech.Items;
using Arcatech.Units;
using com.cyborgAssets.inspectorButtonPro;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Arcatech.UI
{
    public class ItemCardComponent : MonoBehaviour, IUnitInventoryView
    {
        // interface 
        public event UnityAction ViewChangedInventory;
        
        public void RefreshView(UnitInventoryModel model)
        {
            _model = model;
        }

        private UnitInventoryModel _model;
        
        // settings

        [Space,SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TextMeshProUGUI itemTitle;
        [SerializeField] private IconContainerUIScript itemIcon;
        [SerializeField] private TextMeshProUGUI itemText;
        [SerializeField] private IconContainerUIScript skillIcon;
        [SerializeField] private TextMeshProUGUI skillText;
        [SerializeField] private TextMeshProUGUI itemDescriptionText;
        //Internal
        private Item _item;

        
        void OnSetItem()
        {
            Debug.Log("On set item. Implement me!");
            // itemTitle.text = _item.Description.ToString();
            //
            // confirmButton.gameObject.SetActive(false);
            //
            // if (_item is Equipment equip) // means it might come with a skill...
            // {
            //     EquipSO c = equip.Config as EquipSO;
            //     var sk = c.Skill;
            //     skillIcon.AssignIcon(sk);
            //     skillText.text = sk.Description.ToString();
            //     confirmButton.gameObject.SetActive(true);
            // }
            //
            // itemIcon.AssignIcon(_item);
            // itemText.text = _item.Config.Description.ToString();

        }
        private void OnValidate()
        {
            Assert.IsNotNull(confirmButton);
            Assert.IsNotNull(cancelButton);
            Assert.IsNotNull(itemTitle);
            Assert.IsNotNull(skillIcon);
            Assert.IsNotNull(skillText);
            Assert.IsNotNull(itemDescriptionText);
            
        }
                
        public Item ItemHeld
        {
            get => _item;
            set
            {
                _item = value;
                Debug.Log($"Load item {_item} into card");
                OnSetItem();
            }
        }


        private void OnEnable()
        {
            cancelButton.onClick.AddListener(OnCancelButton);
            confirmButton.onClick.AddListener(OnConfirmButton);

            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                var inv = player.GetComponent<EntityInventoryComponent>();
                inv.SetModelView(this);
            }
            else
            {
                Debug.LogError("Player not found");
            }
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            cancelButton.onClick.RemoveListener(OnCancelButton);
            confirmButton.onClick.RemoveListener(OnConfirmButton);
        }


        #region debug
        
        [Space, Header("Debug"), SerializeField] private ItemSO _debugItem;
        [ProButton]
        void DebugItem()
        {
            ItemHeld = _debugItem.BuildItem(null) as Item;
        }
        #endregion
        
        void OnConfirmButton()
        {
            if (ItemHeld is Equipment e)
            {
                _model.EquipEquipment(e, out _);
                ViewChangedInventory?.Invoke();
            }
        }

        void OnCancelButton()
        {            
            ItemHeld = null;
            gameObject.SetActive(false);
        }
        
        

    }
}