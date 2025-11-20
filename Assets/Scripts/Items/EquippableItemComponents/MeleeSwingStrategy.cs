using Arcatech.Actions;
using Arcatech.Triggers;
using Arcatech.Units;
using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Items
{
    public class MeleeSwingStrategy : WeaponStrategy
    {        
        private ActionResult[] OnValidHit { get; }
        private ActionResult[] OnInvalidHit { get; } // for wall hits etc
        public MeleeSwingStrategy(SerializedActionResult[] onHit, SerializedActionResult[] onFailHit,BaseGameEntityComponent unit, 
            WeaponSO cfg, int charges, float reload, EquipmentComponent comp) : base(unit, cfg, charges, reload, 0.05f, comp)
        {

            foreach (var pr in HitSource.GetTriggerNotificationProviders)
            {
                pr.RegisterReceiver(this);
            }
            
            OnValidHit = new ActionResult[onHit.Length];

            for (int i = 0; i < onHit.Length; i++)
            {
                OnValidHit[i] = onHit[i].BuildActionResult();
            }
            
            OnInvalidHit = new ActionResult[onFailHit.Length];
            for (int i = 0; i < onFailHit.Length; i++)
            {
                OnInvalidHit[i] = onFailHit[i].BuildActionResult();
            }
        }
        public override bool UseUsable()
        {
            hitsThisSwing.Clear();
            HitSource.Active = true;
            GameObjectComponent.StartUse();
            return base.UseUsable();
        }

        List<BaseGameEntityComponent> hitsThisSwing = new();

        private void PerformOnHit(EntityStateMachineComponent user, BaseGameEntityComponent target, Transform place)
        {
            foreach (var res in OnValidHit)
            {
                res.ProduceResult(user.GetMainEntity, target, place);
            }
        }

        public override void TriggerEntered(BaseGameEntityComponent enterComponent, ITriggerNotificationProvider trigger)
        {
            if (enterComponent == Owner.GetMainEntity) return;
            if (!hitsThisSwing.Contains(enterComponent))
            {
                PerformOnHit(Owner, enterComponent, GameObjectComponent.EffectSpawn);
                hitsThisSwing.Add(enterComponent);
            }
        }
        public override void TriggerExited(BaseGameEntityComponent enterComponent, ITriggerNotificationProvider trigger)
        {
            // noop 
        }

        public override void StopUsingUsable()
        {
            HitSource.Active = false;
            hitsThisSwing.Clear();
            GameObjectComponent.StopUse();
        }
    }


}