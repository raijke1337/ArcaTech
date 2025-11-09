using Arcatech.Actions;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Items
{
    public class ShootProjectilesStrategy : WeaponStrategy
    {
        private ProjectilesShooterComponent shooter;
        private SerializedProduceProjectileResult projectile;
        
        

        public ShootProjectilesStrategy(SerializedProduceProjectileResult p, SerializedUnitState act, 
            BaseGameEntityComponent unit, WeaponSO cfg, int charges, float reload, float intcd, 
            EquipmentComponent comp) : base(act, unit, cfg, charges, reload, intcd, comp)
        {
            this.projectile = p;
            HitSource.GetTriggerNotificationProvider.RegisterReceiver(this);
        }
    }
}