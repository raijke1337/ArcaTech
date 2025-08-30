using Arcatech.EventBus;
using Arcatech.Units;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Triggers
{
    public class WeaponTriggerComponent : BaseTrigger
    {

        public event UnityAction<Collider> SomeColliderWasHitEvent = delegate { };
        public void ToggleCollider(bool isEnable)
        {
            // Collider.enabled = isEnable;
            // TODO FIX THIS
            Collider.enabled = true;
        }


        protected override void OnTriggerEnter(Collider other)
        {
            SomeColliderWasHitEvent?.Invoke(other);
        }
        // weapon signals about trigger hits based on the event
        // all logic moved to strategy
    }

}