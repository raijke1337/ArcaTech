using Arcatech.Actions;
using Arcatech.Triggers;
using Arcatech.Units;
using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Items
{
    public class MeleeWeaponStrategy : WeaponStrategy
    {
        public MeleeWeaponStrategy(SerializedActionResult[] onHit, SerializedUnitState act, BaseGameEntityComponent unit, WeaponSO cfg, int charges, float reload, BaseEquipmentComponent comp) : base(act, unit, cfg, charges, reload, 0.05f, comp)
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
        protected UnitState CurrentState;

        public override bool TryUseUsable(out UnitState state)
        {

            // TODO needs debug
            // add checks to prevent additional triggering

            bool ok = CanUseUsable();
            state = null;
            if (!ok)
            {
                if (Owner.GetMainEntity.ShowingDebugs) Debug.Log($"Can't use weapon because CD");
                return false;
            }
            hitsThisSwing.Clear();

            // case advancing
           if (CurrentState != null && CurrentState.CanAdvance(out var next))
            {
                state = next.DeserializeState(Owner,GameObjectComponent.Spawner);
                ChargesLogicOnUse();
                CurrentState.ActionStateChangedEvent -= Action_ActionStateChangedEvent;
                CurrentState = state;
                if (Owner.GetMainEntity.ShowingDebugs) Debug.Log($"Advancing weapon combo {next}");
                return true;
            }
            // case first attack OR previous attack is completed
            
           else
            {
                ChargesLogicOnUse();
                state = InitialState;
                CurrentState = state;
                if (Owner.GetMainEntity.ShowingDebugs) Debug.Log($"Starting weapon combo {state}");
                state.ActionStateChangedEvent += Action_ActionStateChangedEvent;
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
                    CurrentState.ActionStateChangedEvent -= Action_ActionStateChangedEvent;
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