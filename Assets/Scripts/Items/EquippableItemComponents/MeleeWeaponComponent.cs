using Arcatech.Triggers;
using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;
namespace Arcatech.Items
{
    [RequireComponent(typeof(WeaponTriggerComponent),typeof(MeleeWeaponTrail))]
    public class MeleeWeaponComponent : BaseWeaponComponent
    {

        public WeaponTriggerComponent Trigger { get => _trigger; }
        public MeleeWeaponTrail Trail { get => _trail; }

        [Child,SerializeField] WeaponTriggerComponent _trigger;
        [Child,SerializeField] MeleeWeaponTrail _trail;

    }

}