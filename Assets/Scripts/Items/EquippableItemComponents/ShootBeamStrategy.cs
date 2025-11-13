using Arcatech.Actions;
using Arcatech.Triggers;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Items
{
    public class ShootBeamStrategy : WeaponStrategy
    {
        ActionResult[] OnColliderHit;

        private LaserEmitterComponent laser;
        public BeamSettings BeamSettings {get; private set;}
        // Settings from scriptable object


        public ShootBeamStrategy(BeamSettings setings, SerializedActionResult[] onHit,  BaseGameEntityComponent unit, 
            WeaponSO cfg, int charges, float reload, EquipmentComponent comp) : base(unit, cfg, charges, reload, 0.05f, comp)
        {
 
            OnColliderHit = new ActionResult[onHit.Length];
            for (int i = 0; i < onHit.Length; i++)
            {
                OnColliderHit[i] = onHit[i].BuildActionResult();
            }

            HitSource.GetTriggerNotificationProvider.RegisterReceiver(this);

            laser = comp.GetComponentInChildren<LaserEmitterComponent>();
            if (!laser)
            {
                Debug.Log($"No laser on {comp.gameObject.name} and it was assigned shoot laser strategy");
            }
            BeamSettings = setings; 
            laser.ConfigureBeam(this);
        }

        public override void TriggerEntered(BaseGameEntityComponent enterComponent, ITriggerNotificationProvider trigger)
        {
            foreach (var res in OnColliderHit)
            {
                res.ProduceResult(Owner.GetMainEntity, enterComponent, enterComponent.EffectSpawn);
            }
        }

        public override void TriggerExited(BaseGameEntityComponent exitComponent, ITriggerNotificationProvider trigger)
        {
            // noop
        }

        public override bool CanUseUsable()
        {
            
            return !laser.Active && base.CanUseUsable();
        }

        public override bool UseUsable()
        {
            bool ok = base.UseUsable();
            if (ok) laser.FireLaser();
            return ok;
        }
    }
}