using Arcatech.Actions;
using Arcatech.Triggers;
using Arcatech.Units;
using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Items
{
    public class MeleeSwingStrategy : WeaponStrategy
    {        
        private IActionResult[] OnColliderHit { get; }
        private UnitState CurrentState;
            
        public MeleeSwingStrategy(SerializedActionResult[] onHit, SerializedUnitState act, BaseGameEntityComponent unit, 
            WeaponSO cfg, int charges, float reload, EquipmentComponent comp) : base(act, unit, cfg, charges, reload, 0.05f, comp)
        {

            HitSource.GetTriggerNotificationProvider.RegisterReceiver(this);
            
            OnColliderHit = new IActionResult[onHit.Length];

            for (int i = 0; i < onHit.Length; i++)
            {
                OnColliderHit[i] = onHit[i].BuildActionResult();
            }

        }


        public override UnitState UseUsable()
        {

            // TODO needs debug
            // add checks to prevent additional triggering
            UnitState state;

            hitsThisSwing.Clear();
            
            //doing manually because at this point it's not subbed to events
            GameObjectComponent.HandleActionState(UnitActionState.Started);
            
            // case advancing
           if (CurrentState != null && CurrentState.CanAdvance(out var next))
            {
                state = next.DeserializeState(Owner,GameObjectComponent.SpawnPoint);
                ChargesLogicOnUse();
                CurrentState.ActionStateChangedEvent -= Action_ActionStateChangedEvent;
                CurrentState = state;
                if (Owner.GetMainEntity.ShowingDebugs) Debug.Log($"Advancing weapon combo {next}");
                return state;
            }
            // case first attack OR previous attack is completed
            
            ChargesLogicOnUse();
            state = InitialState;
            CurrentState = state;
            if (Owner.GetMainEntity.ShowingDebugs) Debug.Log($"Starting weapon combo {state}");
            state.ActionStateChangedEvent += Action_ActionStateChangedEvent;
            return state;
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

        public override void TriggerEntered(BaseGameEntityComponent enterComponent, ITriggerNotificationProvider trigger)
        {
          //  Debug.Log("Bonk noticed");
            if (enterComponent == Owner.GetMainEntity) return;
            if (!hitsThisSwing.Contains(enterComponent))
            {
                PerformOnHit(Owner, enterComponent, GameObjectComponent.SpawnPoint);
                hitsThisSwing.Add(enterComponent);
            }
        
        }

        public override void TriggerExited(BaseGameEntityComponent enterComponent, ITriggerNotificationProvider trigger)
        {
            // noop 
        }
    }


}