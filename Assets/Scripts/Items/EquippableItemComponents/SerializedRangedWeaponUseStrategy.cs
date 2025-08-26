using Arcatech.Triggers;
using Arcatech.Units;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Items
{
    [CreateAssetMenu(fileName = "Ranged Weapon Use Strategy", menuName = "Items/Use strategy/Ranged Weapon")]
    public class SerializedRangedWeaponUseStrategy : SerializedWeaponUseStrategy
    {
       // [SerializeField] protected SerializedProjectileConfiguration Projectile;
       // projectiles are now spawned as "action result"
        public override WeaponStrategy ProduceStrategy(ActiveGameUnitComponent unit, WeaponSO cfg, BaseWeaponComponent comp)
        {
            return new RangedWeaponStrategy(Action,  unit, cfg, TotalCharges, ChargeRestoreTime, InternalCooldown, comp);
        }

    }
}