using Arcatech.Actions;
using Arcatech.Triggers;
using Arcatech.Units;
using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Items
{
    public class MeleeWeaponStrategy : WeaponStrategy, ITriggerNotificationReceiver
    {
        public MeleeWeaponStrategy(SerializedActionResult[] onHit, SerializedUnitState act, BaseGameEntityComponent unit, WeaponSO cfg, int charges, float reload, EquipmentComponent comp) : base(act, unit, cfg, charges, reload, 0.05f, comp)
        {

            //Debug.Log("Melee strategy OK");
            if (comp is MeleeWeaponBaseEquipmentComponent weapon)
            {
                weapon.TriggerTracker.RegisterReceiver(this);
                
                OnColliderHit = new IActionResult[onHit.Length];

                for (int i = 0; i < onHit.Length; i++)
                {
                    OnColliderHit[i] = onHit[i].BuildActionResult();
                }
            }
            


        }
        private IActionResult[] OnColliderHit { get; }
        private UnitState CurrentState;

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
            
            //doing manually because at this point it's not subbed to events
            GameObjectComponent.HandleActionState(UnitActionState.Started);
            
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
            
            ChargesLogicOnUse();
            state = InitialState;
            CurrentState = state;
            if (Owner.GetMainEntity.ShowingDebugs) Debug.Log($"Starting weapon combo {state}");
            state.ActionStateChangedEvent += Action_ActionStateChangedEvent;
            return true;
        }

        private void Action_ActionStateChangedEvent(UnitActionState state)
        {
            GameObjectComponent.HandleActionState(state);
            switch (state)
            {
                case UnitActionState.Completed:
                    CurrentState.ActionStateChangedEvent -= Action_ActionStateChangedEvent;
                    break;
            }
        }

        List<BaseGameEntityComponent> hitsThisSwing = new();

        private void PerformOnHit(ActiveGameUnitComponent user, BaseGameEntityComponent target, Transform place)
        {
            foreach (var res in OnColliderHit)
            {
                res.ProduceResult(user.GetMainEntity, target, place);
            }
        }

        public void TriggerEntered(BaseGameEntityComponent enterComponent, TriggerTrackerComponent trigger)
        {
            //Debug.Log("Bonk noticed");
            if (enterComponent == Owner.GetMainEntity) return;
            if (!hitsThisSwing.Contains(enterComponent))
            {
                PerformOnHit(Owner, enterComponent, GameObjectComponent.Spawner);
                hitsThisSwing.Add(enterComponent);
            }
        
        }

        public void TriggerExited(BaseGameEntityComponent enterComponent, TriggerTrackerComponent trigger)
        {
            // noop 
        }
    }


}