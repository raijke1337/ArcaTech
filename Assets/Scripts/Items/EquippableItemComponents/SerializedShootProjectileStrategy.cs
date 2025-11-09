using Arcatech.Actions;
using UnityEngine;

namespace Arcatech.Items
{
    [CreateAssetMenu(fileName = "Shoot projectiles strategy", menuName = "Items/Use strategy/Shoot projectiles")]
    public class SerializedShootProjectileStrategy : SerializedWeaponUseStrategy
    {
        [SerializeField] private SerializedProduceProjectileResult projectile;
        public override WeaponStrategy ProduceStrategy(BaseGameEntityComponent unit, WeaponSO cfg, EquipmentComponent comp)
        {
            return new ShootProjectilesStrategy(projectile,state,unit,cfg,TotalCharges,ChargeRestoreTime,InternalCooldown,comp);
        }
    }

}