using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Triggers
{
    [RequireComponent(typeof(Collider))]
    public class BaseTrigger : ValidatedMonoBehaviour
    {
        public Collider Collider => _collider;
        [SerializeField, Self] Collider _collider;
        private void OnEnable()
        {
            if (!_collider.isTrigger)
            {
                _collider.isTrigger = true;
            }
        }
        


        protected virtual void OnTriggerEnter(Collider other) { }
        protected virtual void OnTriggerExit(Collider other) { }



    }

}