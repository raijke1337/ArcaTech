using System.Collections.Generic;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Interactions
{
    public class KillComponentsInteraction : InteractionEventHandlerBase, IKillerComponent
    {
        [SerializeField, ReadOnlyText] private string killedComponents;

        private List<IKillableComponent> toKill;
        private void Start()
        {
            toKill = new List<IKillableComponent>(GetComponentsInChildren<IKillableComponent>());
            killedComponents = toKill.ToString()+" components will be killed";
        }


        public override void DoInteraction(bool success, IInteractor interactor)
        {
            if (!success) return;
            foreach (var component in toKill)
            {
                component.SetKilled(this,true);   
            }
        }


        public string KilledBy => $"Interaction handler {name}";
    }
}