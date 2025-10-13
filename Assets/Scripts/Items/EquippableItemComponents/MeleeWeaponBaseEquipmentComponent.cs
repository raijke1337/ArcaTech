using System;
using Arcatech.Triggers;
using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;
namespace Arcatech.Items
{
    [RequireComponent(typeof(WeaponTriggerComponent),typeof(MeleeWeaponTrail))]
    public class MeleeWeaponBaseEquipmentComponent : BaseEquipmentComponent
    {

        public WeaponTriggerComponent Trigger { get => trigger; }


        [Child,SerializeField] WeaponTriggerComponent trigger;
        [Child,SerializeField] MeleeWeaponTrail trail;

        private void Start()
        {
            trigger.ToggleCollider(false);
            trail.Emit = false;
        }

        public override void HandleActionState(UnitActionState s)
        {
            base.HandleActionState(s);
            switch (s)
            {
                case UnitActionState.None:
                    break;
                case UnitActionState.Started:
                    trigger.ToggleCollider(true);
                    trail.Emit = true;
                    break;
                case UnitActionState.ExitTime:
                    break;
                case UnitActionState.Completed:
                    trigger.ToggleCollider(false);
                    trail.Emit = false;
                    break;
            }
        }
    }

}