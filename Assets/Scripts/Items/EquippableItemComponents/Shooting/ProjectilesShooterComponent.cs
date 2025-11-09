using System;
using Arcatech.Triggers;
using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Items
{
    public class ProjectilesShooterComponent : RangedWeaponShooterComponent
    {

        public ITriggerNotificationProvider GetTriggerNotificationProvider { get; protected set; }
    }
    
    public abstract class RangedWeaponShooterComponent : ValidatedMonoBehaviour
    {
    
    }
}