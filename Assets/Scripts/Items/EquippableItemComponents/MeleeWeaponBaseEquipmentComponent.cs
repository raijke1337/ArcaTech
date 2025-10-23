using System;
using Arcatech.Triggers;
using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;
namespace Arcatech.Items
{
    [RequireComponent(typeof(TriggerTrackerComponent),typeof(MeleeWeaponTrail))]
    public class MeleeWeaponBaseEquipmentComponent : EquipmentComponent
    {

        public TriggerTrackerComponent TriggerTracker => triggerTracker;


        [Child,SerializeField] TriggerTrackerComponent triggerTracker;
        [Child,SerializeField] MeleeWeaponTrail trail;

        private void Start()
        {
            trail.Use = false;
        }

        public override void HandleActionState(UnitActionState s)
        {
            switch (s)
            {
                case UnitActionState.None:
                    break;
                case UnitActionState.Started:
                    triggerTracker.Active = true;
                    trail.Use = true;
                    break;
                case UnitActionState.ExitTime:
                    break;
                case UnitActionState.Completed:
                    triggerTracker.Active = false;
                    trail.Use = false;
                    break;
            }
        }
    }

}