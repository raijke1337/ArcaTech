using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Arcatech.Lewding
{
    [RequireComponent(typeof(Collider))]
    public class TouchZone : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
    {
        [SerializeField] TouchZoneType place;
        public event UnityAction<TouchZoneType,Vector3> Touch = delegate { };

        public void OnPointerClick(PointerEventData eventData)
        {
            Touch?.Invoke(place,eventData.pointerPressRaycast.worldPosition);
        }

        public void OnPointerEnter(PointerEventData eventData)
        { }
    }
}