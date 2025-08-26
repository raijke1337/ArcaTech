using Arcatech.Actions;
using Arcatech.Triggers;
using Arcatech.Units;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Arcatech.Items
{
    public class MeleeWeaponStrategy : WeaponStrategy
    {
        public MeleeWeaponStrategy(SerializedActionResult[] onHit, SerializedUnitAction act, ActiveGameUnitComponent unit, WeaponSO cfg, int charges, float reload, BaseWeaponComponent comp) : base(act, unit, cfg, charges, reload, 0.05f, comp)
        {
            Trigger = (comp as MeleeWeaponComponent).Trigger;
            Trigger.SomeColliderWasHitEvent += HandleColliderHitEvent;
            Trigger.ToggleCollider(false);

            Trail = (comp as MeleeWeaponComponent).Trail;

            OnColliderHit = new IActionResult[onHit.Length];

            for (int i = 0; i < onHit.Length; i++)
            {
                OnColliderHit[i] = onHit[i].BuildActionResult();
            }

        }
        protected WeaponTriggerComponent Trigger;
        protected MeleeWeaponTrail Trail;
        protected IActionResult[] OnColliderHit { get; }
        protected BaseUnitAction currentAction;
        public async void SwitchCollider(bool state, float delay)
        {
            Trail.Emit = state;
            await Task.Delay((int)delay*1000);
            Trigger.ToggleCollider(state);
            if (Owner.GetMainEntity.ShowingDebugs) Debug.Log($"collider on {WeaponComponent} {(state == true ? "on" : "off")} ");
        }

        public override bool TryUseUsable(out BaseUnitAction action)
        {
            // TODO needs debug
            // add checks to prevent additional triggering

            bool ok = CanUseUsable();
            action = null;
            if (!ok)
            {
                if (Owner.GetMainEntity.ShowingDebugs) Debug.Log($"Can't use weapon because CD");
                return false;
            }
            hitsThisSwing.Clear();

            /// case advancing
           if (currentAction != null && currentAction.CanAdvance(out var next))
            {
                action = next.ProduceAction(Owner,WeaponComponent.Spawner);
                ChargesLogicOnUse();
                currentAction = action;
                if (Owner.GetMainEntity.ShowingDebugs) Debug.Log($"Advancing weapon combo {next}");
                return true;
            }
            //// case first attack OR previous attack is completed
            
           else
            {
                ChargesLogicOnUse();
                action = Action;
                currentAction = action;
                if (Owner.GetMainEntity.ShowingDebugs) Debug.Log($"Starting weapon combo {action}");
                return true;
            }
        }


        List<BaseGameEntityComponent> hitsThisSwing = new();
        private void HandleColliderHitEvent(Collider target)
        {
            if (target == Owner) return;
            else
            {
                if (target.TryGetComponent<BaseGameEntityComponent>(out var e))
                {
                    if (!hitsThisSwing.Contains(e))
                    {
                        PerformOnHit(Owner, e, WeaponComponent.Spawner);
                        hitsThisSwing.Add(e);
                    }
                }
            }
        }
        protected void PerformOnHit(ActiveGameUnitComponent user, BaseGameEntityComponent target, Transform place)
        {
            foreach (var res in OnColliderHit)
            {
                res.ProduceResult(user.GetMainEntity, target, place);
            }
        }
    }


}