using Arcatech.EventBus;
using Arcatech.Units;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Arcatech
{
    public class LittlePauseHelperComponent : MonoBehaviour
    {
        EventBinding<PauseToggleEvent> _eventBinding;
        List<IPausableComponent> _components;

        private void OnEnable() 
        {
            _eventBinding = new EventBinding<PauseToggleEvent>(OnPauseCommand);
            EventBus<PauseToggleEvent>.Register(_eventBinding);
            _components = GetComponentsInChildren<IPausableComponent>().ToList();
        }

        private void OnApplicationPause(bool pause)
        {

            foreach (var component in _components)
            {
                component.Paused = pause;
            }
            //Debug.Log(pause ? "Pausing" : "Resuming"+$" {_components.Count} components");

        }
        private void OnPauseCommand(PauseToggleEvent pause)
        {
            OnApplicationPause(pause.Value);
        }
        private void OnDisable()
        {
            EventBus<PauseToggleEvent>.Deregister(_eventBinding);
        }
    }
}