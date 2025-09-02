using Arcatech.EventBus;
using Arcatech.Triggers;
using KBCore.Refs;
using UnityEngine;
namespace Arcatech
{/// <summary>
 /// new component that defines any game enitity that does something
 /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class BaseGameEntityComponent : ValidatedMonoBehaviour
    {
        [SerializeField] string _name;
        [SerializeField] Side entitySide;
        [Space,SerializeField] protected bool _showDebugs = false;

        public string GetName { get => _name; }
        public Side GetEntitySide => entitySide;
        public bool ShowingDebugs => _showDebugs;

        public bool Paused { get => _paused; }
        protected bool _paused;
        [Space,Header("Rigidbody override"),SerializeField,Self] Rigidbody _rigidbody;
        [SerializeField] bool gravity = false;
        [SerializeField] bool usePhysics = false;

        EventBinding<PauseToggleEvent> _pauseBind;
        private void OnEnable()
        {
            _pauseBind = new EventBinding<PauseToggleEvent>(HandlePauseEvent);
            EventBus<PauseToggleEvent>.Register(_pauseBind);
            _rigidbody.useGravity = gravity;
            _rigidbody.isKinematic = !usePhysics;
        }

        void HandlePauseEvent(PauseToggleEvent e)
        {
            _paused = e.Value;
        }

        private void OnDisable()
        {
            EventBus<PauseToggleEvent>.Deregister(_pauseBind);
        }
    }
}