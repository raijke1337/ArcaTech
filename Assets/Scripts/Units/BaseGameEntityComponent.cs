using Arcatech.EventBus;
using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;
namespace Arcatech
{/// <summary>
 /// new component that defines any game enitity that does something
 /// </summary>
    [RequireComponent(typeof(Rigidbody),typeof(Collider))]
    public class BaseGameEntityComponent : ValidatedMonoBehaviour
    {
        [SerializeField] string _name;
        [SerializeField] Side entitySide;
        [Space,SerializeField] protected bool _showDebugs = false;

        
        public string GetName { get => _name; }
        public Side GetEntitySide => entitySide;
        public bool ShowingDebugs => _showDebugs;
        public Collider Collider { get; protected set; }

        [Space,Header("Rigidbody override"),SerializeField,Self] Rigidbody _rigidbody;
        [SerializeField] bool gravity = false;
        [SerializeField] bool usePhysics = false;

        EventBinding<PauseToggleEvent> _pauseBind;

        protected override void OnValidate()
        {
            base.OnValidate();
            gameObject.layer = LayerMask.NameToLayer("Entities");
            Collider = GetComponent<Collider>(); 
        }

        private void OnEnable()
        {
            _rigidbody.useGravity = gravity;
            _rigidbody.isKinematic = !usePhysics;
        }
        
            
/// <summary>
/// TODO rewrite this to use the new interfaces (ipausalbe, ikillable) etc
/// </summary>
        public bool Paused { get => _paused;  }
        protected bool _paused;
        
    }
}