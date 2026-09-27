using System;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Interactions
{
    public sealed class OverrideDrawStrategyEffect : InteractionEffect
    {
        [SerializeField] private DrawItemsStrategy interactorStrategy;
        [SerializeField] private DrawItemsStrategy targetStrategy;
        public override void Play(InteractionContext ctx)
        {
            if (interactorStrategy != null)
            {
                if (ctx.Interactor.Entity.TryGetComponent(out EntityInventoryDrawerComponent comp))
                {
                    comp.OverrideDrawStrategy(interactorStrategy);   
                }
            }

            if (targetStrategy != null)
            {
                if (ctx.Target.TryGetComponent(out EntityInventoryDrawerComponent comp))
                {
                    comp.OverrideDrawStrategy(targetStrategy);
                }
            }
        }
    }
}