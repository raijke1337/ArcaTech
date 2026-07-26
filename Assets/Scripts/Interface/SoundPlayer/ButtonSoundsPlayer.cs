using Arcatech.Audio;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Arcatech.UI
{
    
[DisallowMultipleComponent]
public class ButtonSoundsPlayer : MonoBehaviour,
        IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler,
        ISelectHandler, IDeselectHandler
    {
        [SerializeField] private SoundDefinition hoverSound;
        [SerializeField] private SoundDefinition pressSound;
        [SerializeField] private SoundDefinition releaseSound;
        [SerializeField] private SoundDefinition clickSound;
        [SerializeField] private SoundDefinition selectSound;
        [SerializeField] private SoundDefinition deselectSound;

        public void OnPointerEnter(PointerEventData e) => AudioEvents.PlayUi(hoverSound);
        public void OnPointerDown(PointerEventData e)  => AudioEvents.PlayUi(pressSound);
        public void OnPointerUp(PointerEventData e)    => AudioEvents.PlayUi(releaseSound);
        public void OnPointerClick(PointerEventData e) => AudioEvents.PlayUi(clickSound);
        public void OnSelect(BaseEventData e)          => AudioEvents.PlayUi(selectSound);
        public void OnDeselect(BaseEventData e)        => AudioEvents.PlayUi(deselectSound);
    }
}
