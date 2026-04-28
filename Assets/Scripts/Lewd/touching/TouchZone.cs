using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Arcatech.Lewding
{
    [RequireComponent(typeof(Collider))]
    public class TouchZone : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
    {
        [SerializeField] TouchZoneType place;
        public event UnityAction<TouchZoneType> Touch = delegate { };

        public void OnPointerClick(PointerEventData eventData)
        {
            Touch.Invoke(place);
            Debug.Log("Clicked "+place);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("Entered "+place);
        }
    }
}