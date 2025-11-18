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

        [SerializeField] private List<SerializedActionResult> serializedResultsSuccess;
        [SerializeField] private List<SerializedActionResult> serializedResultsFail;
        private List<ActionResult> _listS;
        private List<ActionResult> _listF;
        
        protected void OnValidate()
        {
            if (serializedResultsSuccess == null &&  serializedResultsFail == null) Debug.LogWarning($"No results set in {this} on {gameObject.name}");
        }

        private void Awake()
        {
            _listS = new List<ActionResult>();
            if (serializedResultsSuccess == null) return;
            foreach (var result in serializedResultsSuccess)
            {
                if (result!= null)
                    _listS.Add(result.BuildActionResult());
            }
            
            _listF = new List<ActionResult>();
            if (serializedResultsFail == null) return;
            foreach (var result in serializedResultsFail)
            {
                if (result!= null)
                    _listF.Add(result.BuildActionResult());
            }
        }

        public override void DoInteraction(bool success, IInteractor interactor, IInteractive item)
        {
            if (success)
            {
                if (item == null)
                {
                    // apply result using only interactor data
                    foreach (var result in _listS)
                    {
                        result.ProduceResult(interactor.InteractionContext.EntityComponent,
                            null,
                            interactor.InteractionContext.ActionTransform);
                    }
                }

                else
                {          
                    // use item as target
                    foreach (var result in _listS)
                    {
                        result.ProduceResult(interactor.InteractionContext.EntityComponent,
                            item.GetBaseComponent,
                            item.GetBaseComponent.EffectSpawn);
                    }
                }
            }
            else
            {
                if (item == null)
                {
                    // apply result using only interactor data
                    foreach (var result in _listF)
                    {
                        result.ProduceResult(interactor.InteractionContext.EntityComponent,
                            null,
                            interactor.InteractionContext.ActionTransform);
                    }
                }

                else
                {          
                    // use item as target
                    foreach (var result in _listF)
                    {
                        result.ProduceResult(interactor.InteractionContext.EntityComponent,
                            item.GetBaseComponent,
                            item.GetBaseComponent.EffectSpawn);
                    }
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
        public void OverrideResults(IEnumerable<ActionResult> results)
        {Debug.Log("OverrideResults");
            _listS = new List<ActionResult>(results);
        }

        public void OverrideResults(ActionResult result)
        {
            Debug.Log("OverrideResults");
            _listS.Add(result); 
        }

        public void RedrawItem(EquipmentComponent toDraw)
        {
            Instantiate(toDraw,transform);
        }

    }
}