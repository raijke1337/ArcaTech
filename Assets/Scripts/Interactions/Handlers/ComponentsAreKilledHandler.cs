using System.Collections.Generic;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Interactions
{
    public class ComponentsAreKilledHandler : InteractionHandlerBase
    {
        [SerializeField, ReadOnlyText] private string killedComponents;

        private List<IKillableComponent> toKill;
        private void Start()
        {
            toKill = new List<IKillableComponent>(GetComponentsInChildren<IKillableComponent>());
            killedComponents = toKill.ToString()+" components will be killed";
        }

        public override void DoInteraction(IInteractor interactor, IInteractive item)
        {
            foreach (var component in toKill)
            {
                component.Killed = true;   
            }
        }

        public override void EndInteraction(IInteractor interactor, IInteractive item = null)
        {
            
        }
    }
}