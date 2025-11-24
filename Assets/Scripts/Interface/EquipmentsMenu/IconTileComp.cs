using Arcatech.Items;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Arcatech.UI
{
    public class IconTileComp :MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        #region ipointer
        public void OnPointerClick(PointerEventData eventData) => CallbackClick(eventData);

        public void OnPointerEnter(PointerEventData eventData) => CallbackEnter(eventData);
        public void OnPointerExit(PointerEventData eventData) => CallbackExit(eventData);
        #endregion



        private Item _item;
        public Item Item
        {
            get
            { return _item; }
            set
            {
                _item = value;
                if (value == null) Clear();
                else SetSprite(value.Description.Picture);
            }
        }

        [SerializeField] protected Image _itemImg;
        [SerializeField] protected Sprite _defaultImg;
        public void SetSprite(Sprite s)
        {
            if (s!=null) _itemImg.sprite = s;
        }


        [SerializeField] private Transform _highlight;
        public void HighLightToggle()
        {
            _highlight.gameObject.SetActive(!_highlight.gameObject.activeSelf);
        }


        public virtual void Clear()
        {
            _itemImg.sprite = _defaultImg;
        }


        public event SimpleEventsHandler<IconTileComp> IconClickedEvent;

        protected virtual void CallbackClick(PointerEventData data)
        {
            IconClickedEvent?.Invoke(this);
        }
        protected virtual void CallbackEnter(PointerEventData data)
        {

        }
        protected virtual void CallbackExit(PointerEventData data)
        {

        }



    }
}