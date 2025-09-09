using Arcatech.Actions;
using Arcatech.Triggers;
using Arcatech.Units;
using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Items
{
    public class MeleeWeaponStrategy : WeaponStrategy
    {
        public MeleeWeaponStrategy(SerializedActionResult[] onHit, SerializedUnitAction act, BaseGameEntityComponent unit, WeaponSO cfg, int charges, float reload, BaseEquipmentComponent comp) : base(act, unit, cfg, charges, reload, 0.05f, comp)
        {
            Trigger = (comp as MeleeWeaponBaseEquipmentComponent).Trigger;
            Trigger.SomeColliderWasHitEvent += HandleColliderHitEvent;
            
            OnColliderHit = new IActionResult[onHit.Length];

            for (int i = 0; i < onHit.Length; i++)
            {
                OnColliderHit[i] = onHit[i].BuildActionResult();
            }

        }
        protected WeaponTriggerComponent Trigger;
        protected IActionResult[] OnColliderHit { get; }
        protected BaseUnitAction currentAction;

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

            // case advancing
           if (currentAction != null && currentAction.CanAdvance(out var next))
            {
                action = next.ProduceAction(Owner,GameObjectComponent.Spawner);
                ChargesLogicOnUse();
                currentAction.ActionStateChangedEvent -= Action_ActionStateChangedEvent;
                currentAction = action;
                if (Owner.GetMainEntity.ShowingDebugs) Debug.Log($"Advancing weapon combo {next}");
                return true;
            }
            // case first attack OR previous attack is completed
            
           else
            {
                ChargesLogicOnUse();
                action = InitialAction;
                currentAction = action;
                if (Owner.GetMainEntity.ShowingDebugs) Debug.Log($"Starting weapon combo {action}");
                action.ActionStateChangedEvent += Action_ActionStateChangedEvent;
                return true;
            }
        }

        private void Action_ActionStateChangedEvent(UnitActionState state)
        {
            GameObjectComponent.HandleActionState(state);
            switch (state)
            {
                case UnitActionState.None:
                    break;
                case UnitActionState.Started:
                    break;
                case UnitActionState.ExitTime:
                    break;
                case UnitActionState.Completed:
                    currentAction.ActionStateChangedEvent -= Action_ActionStateChangedEvent;
                    break;
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
                        PerformOnHit(Owner, e, GameObjectComponent.Spawner);
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