using System;
using System.Collections.Generic;
using Arcatech.Actions;
using Arcatech.Items;
using NUnit.Framework;
using UnityEngine;

namespace Arcatech.Interactions
{
    public class GenericActionResultInteraction : InteractionEventHandlerBase
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
                    _listS.Add(result.Deserialize());
            }
            
            _listF = new List<ActionResult>();
            if (serializedResultsFail == null) return;
            foreach (var result in serializedResultsFail)
            {
                if (result!= null)
                    _listF.Add(result.Deserialize());
            }
        }

        public override void DoInteraction(bool success, IInteractor interactor)
        {
            throw new NotImplementedException();
            // if (success)
            // {
            //     foreach (var result in _listS)
            //     {
            //         result.ProduceResult(interactor.InteractionContext.CurrentInteractive.GetBaseComponent,
            //             interactor.InteractionContext.EntityComponent,
            //             interactor.InteractionContext.CurrentInteractive.GetBaseComponent.EffectSpawn.transform.position, 
            //             interactor.InteractionContext.CurrentInteractive.GetBaseComponent.EffectSpawn.transform.rotation);
            //     }
            // }
            // else
            // {
            //     foreach (var result in _listF)
            //     {
            //         result.ProduceResult(interactor.InteractionContext.CurrentInteractive.GetBaseComponent,
            //             interactor.InteractionContext.EntityComponent,
            //             interactor.InteractionContext.CurrentInteractive.GetBaseComponent.EffectSpawn.transform.position,
            //             interactor.InteractionContext.CurrentInteractive.GetBaseComponent.EffectSpawn.transform.rotation);
            //     }
            // }
        }

    }
}