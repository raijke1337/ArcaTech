using System.Collections;
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
        }
        private void OnPauseCommand(PauseToggleEvent pause)
        {
            OnApplicationPause(pause.Value);
        }
        private void OnDisable()
        {
            EventBus<PauseToggleEvent>.Deregister(_eventBinding);
        }

        public void Pause(float time) => StartCoroutine(TimedPause(time));

        private IEnumerator TimedPause(float time)
        {
            OnApplicationPause(true);
            Debug.Log("Pause");
            yield return new WaitForSeconds(time);
            Debug.Log("unPause");
            OnApplicationPause(false);
        }
    }
}