using Arcatech.Actions;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Items
{
   // public class ShootProjectilesStrategy : WeaponStrategy
    //{
    //     private ProjectilesShooterComponent shooter;
    //     private SerializedProduceProjectileResult projectile;
    //
    //
    //
    //     public ShootProjectilesStrategy(SerializedProduceProjectileResult p,
    //         BaseGameEntityComponent unit, EquipWithUsablesSO cfg, int charges, float reload, float intcd,
    //         EquipmentComponent comp) : base(unit, cfg, charges, reload, intcd, comp)
    //     {
    //         this.projectile = p;
    //         foreach (var t in HitSource.GetTriggerNotificationProviders)
    //         {
    //             t.RegisterReceiver(this);
    //         }
    //     
    // }
    //
    //     public override bool UseUsable()
    //     {
    //         var ok = base.UseUsable();
    //         if (ok)
    //         {
    //             projectile.BuildActionResult().ProduceResult(Owner.GetMainEntity, null, SpawnPoint);
    //         }
    //         return ok;
    //     }
    // }
}