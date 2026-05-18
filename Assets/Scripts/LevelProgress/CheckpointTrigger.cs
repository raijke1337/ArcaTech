using System;
using Arcatech.Interactions;
using UnityEngine;

namespace Arcatech.SaveSystem
{
    public class CheckpointTrigger : InteractionEffect
    {
        public bool ReadItemState { get; private set; }
        public void OnWriteItemState(bool state, LevelProgressManager manager)
        {
            ReadItemState = state;
            if (state)
            {
                gameObject.SetActive(false);
            }
        }

        public override void Play(InteractionContext ctx)
        {
            if (ctx.Interactor == null) return; // on level load
            if (ctx.Interactor.Entity.CompareTag("Player"))
            {
                LevelProgressManager.Instance.OnCheckPointReached(this);
            }
        }
    }
}