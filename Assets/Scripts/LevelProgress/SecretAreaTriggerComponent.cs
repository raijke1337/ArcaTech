using System;
using Arcatech.Interactions;
using Arcatech.Triggers;
using KBCore.Refs;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Arcatech.SaveSystem
{
    public class SecretAreaTriggerComponent : InteractionEffect
    {
        public override void Play(InteractionContext ctx)
        {
            if (ctx.Interactor.Entity.CompareTag("Player"))
            {
                Debug.Log("Secret Area found!");
            }
        }

        public override void OnLoadLevelState(ProgressItemState stateToLoad)
        {
            if (stateToLoad ==  ProgressItemState.Completed) gameObject.SetActive(false);
        }
    }
}