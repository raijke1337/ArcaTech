
using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;
namespace Arcatech
{/// <summary>
 /// new component that defines any game enitity that does something
 /// </summary>
    [RequireComponent(typeof(Rigidbody),typeof(Collider),typeof(LittlePauseHelperComponent))]
    public class BaseGameEntityComponent : ValidatedMonoBehaviour
    {
        [SerializeField, Self]
        LittlePauseHelperComponent _pauser;

        [Space,SerializeField] string _name;
        [SerializeField] Side entitySide;
        [Space,SerializeField] protected bool _showDebugs = false;
        
        
        public string GetName { get => _name; }
        public Side GetEntitySide => entitySide;
        public bool ShowingDebugs => _showDebugs;
        public Collider Collider { get; protected set; }

        [Space,Header("Rigidbody override"),SerializeField,Self] Rigidbody _rigidbody;
        [SerializeField] bool gravity = false;
        [SerializeField] bool usePhysics = false;


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
    }
}