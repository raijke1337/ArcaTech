using System;
using Arcatech.Triggers;
using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;
namespace Arcatech.Items
{
    [RequireComponent(typeof(TriggerTrackerComponent),typeof(MeleeWeaponTrail))]
    public class MeleeWeaponBaseEquipmentComponent : BaseEquipmentComponent
    {

        public TriggerTrackerComponent TriggerTracker => triggerTracker;


        [Child,SerializeField] TriggerTrackerComponent triggerTracker;
        [Child,SerializeField] MeleeWeaponTrail trail;

        private void Start()
        {
            triggerTracker.enabled = false;
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
                    triggerTracker.enabled = true;
                    trail.Emit = true;
                    break;
                case UnitActionState.ExitTime:
                    break;
                case UnitActionState.Completed:
                    triggerTracker.enabled = false;
                    trail.Emit = false;
                    break;
            }
        }
    }

}