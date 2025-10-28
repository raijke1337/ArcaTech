using System;
using System.Collections.Generic;
using Arcatech.Actions;
using Arcatech.Items;
using NUnit.Framework;
using UnityEngine;

namespace Arcatech.Interactions
{
    public class ActionResultIsProducedInteractionHandler : InteractionHandlerBase
    {

        [SerializeField] private List<SerializedActionResult> serializedResults;
        private List<IActionResult> _list;
        protected void OnValidate()
        {
            Assert.IsNotNull(serializedResults);
            Assert.IsNotEmpty(serializedResults);
        }

        private void Awake()
        {
            _list = new List<IActionResult>();
            if (serializedResults == null) return;
            foreach (var result in serializedResults)
            {
                if (result!= null)
                _list.Add(result.BuildActionResult());
            }
        }

        public override void DoInteraction(IInteractor interactor, IInteractive item)
        {

            if (item == null)
            {
                // apply result using only interactor data
                foreach (var result in _list)
                {
                    result.ProduceResult(interactor.InteractionContext.ActiveGameUnitComponent.GetMainEntity,
                        null,
                        interactor.InteractionContext.ActionTransform);
                }
            }

            else
            {          
                // use item as target
                foreach (var result in _list)
                {
                    result.ProduceResult(interactor.InteractionContext.ActiveGameUnitComponent.GetMainEntity,
                        item.GetBaseComponent,
                        item.GetBaseComponent.SpawnPoint);
                }
            }

        }

        public override void EndInteraction(IInteractor interactor, IInteractive item = null)
        {
            // todo add "OnEndInteraction" results if necessary
        }


        /// <summary>
        /// I use this when instantiating a "dropped item" interactive component.
        /// </summary>
        /// <param name="results"></param>
        public void OverrideResults(IEnumerable<IActionResult> results)
        {Debug.Log("OverrideResults");
            _list = new List<IActionResult>(results);
        }

        public void OverrideResults(IActionResult result)
        {
            Debug.Log("OverrideResults");
            _list.Add(result); 
        }

        public void RedrawItem(EquipmentComponent toDraw)
        {
            Instantiate(toDraw,transform);
        }

    }
}