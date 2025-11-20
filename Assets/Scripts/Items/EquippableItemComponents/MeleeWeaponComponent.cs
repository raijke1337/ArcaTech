using System;
using Arcatech.Triggers;
using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;
namespace Arcatech.Items
{
    [RequireComponent(typeof(TriggerTrackerComponent))]
    public class MeleeWeaponComponent : BaseWeaponComponent
    {
        
        public override void StartUse()
        {
            base.StartUse();
            
        }

        public override void StopUse()
        {
            base.StopUse();
            
        }
    }
}