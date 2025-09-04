using System;
using System.Collections.Generic;
using Arcatech.Actions;
using NUnit.Framework;
using UnityEngine;

namespace Arcatech.Interactions
{
    public class ActionResultIsProducedInteractionHandler : InteractionHandlerBase
    {

        [SerializeField] private List<SerializedActionResult> _results;

        private List<IActionResult> _list;
        protected override void OnValidate()
        {
            base.OnValidate();
            Assert.IsNotNull(_results);
            Assert.IsNotEmpty(_results);
        }

        private void Awake()
        {
            _list = new List<IActionResult>();
            foreach (var result in _results)
            {
                _list.Add(result.BuildActionResult());
            }
        }

        public override void DoInteraction(IInteractor interactor, IInteractive item, IInteractionContext context)
        {
            foreach (var result in _list)
            {
                result.ProduceResult(context.ActiveGameUnitComponent.GetMainEntity,item.GetBaseComponent,context.ActionTransform);
            }
        }
    }
}