using Arcatech.Items;
using Arcatech.Skills;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Units
{
    public abstract class ArmedUnit : EquippedUnitOLD
    {

        [Space,Header ("Armed Unit")]
        [SerializeField] protected WeaponController _weapons;
        [SerializeField] protected SkillsController _skills;

        public override void StartControllerUnit()
        {
            base.StartControllerUnit();

            _weapons = new WeaponController(_stats,_inventory, this);
            _weapons.StartController();

            _skills = new SkillsController(_stats, _inventory, this);
            _skills.StartController();

        }

        public override void DisableUnit()
        {
            base.DisableUnit();
            _weapons.StopController();
            _skills.StopController();
        }

        public override void RunFixedUpdate(float delta)
        {

            base.RunFixedUpdate(delta);
            _skills.FixedControllerUpdate(delta);
            _weapons.FixedControllerUpdate(delta);
        }
        public override void RunUpdate(float delta)
        {
            base.RunUpdate(delta);
            _skills.ControllerUpdate(delta);
            _weapons.ControllerUpdate(delta);
        }
        public bool IsArmed (out IWeapon currentWeapon)
        {
            return _weapons.CheckUnitArmed(out currentWeapon);
        }

    }


}