﻿using Arcatech.Triggers;
using Arcatech.Units;

namespace Arcatech.Items
{
    public class RangedWeaponUseStrategy : BaseWeaponUseStrategy
    {
        RangedWeaponComponent weapon;

        public RangedWeaponUseStrategy(RangedWeaponComponent weapon, DummyUnit owner, SerializedStatsEffectConfig[] effects, IWeapon w) : base(owner, effects,w) {
        
            this.weapon = weapon;
        }

        public override void WeaponUsedStateEnter()
        {
            weapon.Projectile.ProduceProjectile(Owner, weapon.transform, EffectConfigs);
        }

        public override void WeaponUsedStateExit()
        {

        }
    }


}