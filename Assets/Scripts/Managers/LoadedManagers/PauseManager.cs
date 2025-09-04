using Arcatech.EventBus;
using UnityEngine;

namespace Arcatech.Managers
{
    public class PauseManager : GenericLazySingleton<PauseManager>
    {
        EventBinding<PauseToggleEvent> _pauseBind;


        private void OnPawsToggle(PauseToggleEvent isPausing)
        {
            Debug.Log($"Paws the game: {isPausing}");
        }



        private void OnEnable()
        {
            _pauseBind = new EventBinding<PauseToggleEvent>(OnPawsToggle);
            EventBus<PauseToggleEvent>.Register(_pauseBind);
        }
        private void OnDisable()
        {
            EventBus<PauseToggleEvent>.Deregister(_pauseBind);
        }

    }

}