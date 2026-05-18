using System;
using Arcatech.Interactions;
using UnityEngine;

namespace Arcatech.SaveSystem
{
    public class CheckpointTrigger : InteractionEffect
    {
        public override void Play(InteractionContext ctx)
        {
            if (ctx.Interactor == null) return; // on level load
            if (ctx.Interactor.Entity.CompareTag("Player"))
            {
                LevelProgressManager.Instance.OnCheckPointReached(this);
            }
        }

        public override void OnLoadLevelState(ProgressItemState stateToLoad)
        {
            if (stateToLoad ==  ProgressItemState.Completed) gameObject.SetActive(false);
        }
    }
}