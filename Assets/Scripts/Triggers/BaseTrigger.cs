using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Triggers
{
    [RequireComponent(typeof(Collider))]
    public abstract class BaseTrigger : ValidatedMonoBehaviour
    {
        public Collider Collider { get; protected set; }

        protected virtual void Awake()
        {
            Collider = GetComponent<Collider>();
            Collider.isTrigger = true;
        }



        protected abstract void OnTriggerEnter(Collider other);
        protected virtual void OnTriggerExit(Collider other) { }



    }

}