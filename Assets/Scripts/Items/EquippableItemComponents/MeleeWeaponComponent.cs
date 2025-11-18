using System;
using Arcatech.Triggers;
using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;
namespace Arcatech.Items
{
    [RequireComponent(typeof(TriggerTrackerComponent),typeof(MeleeWeaponTrail))]
    public class MeleeWeaponComponent : BaseWeaponComponent
    {
        [Child,SerializeField] MeleeWeaponTrail trail;

        private void Start()
        {
            trail.Use = false;
        }

        public override void HandleActionState(UnitActionState s)
        {
           // Debug.Log($"{this} state {s}");
            switch (s)
            {
                case UnitActionState.None:
                    break;
                case UnitActionState.Started:
                    foreach (var p in GetTriggerNotificationProviders)
                    {
                        p.Active = true;
                    }
                    trail.Use = true;
                    break;
                case UnitActionState.ExitTime:
                    break;
                case UnitActionState.Completed:
                    foreach (var p in GetTriggerNotificationProviders)
                    {
                        p.Active = false;
                    }
                    trail.Use = false;
                    break;
            }
        }

    }
}