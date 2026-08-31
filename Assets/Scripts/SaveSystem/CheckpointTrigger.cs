using System;
using Arcatech.Triggers;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.SaveSystem
{
    [RequireComponent(typeof(TriggerTrackerComponent))]
    public class CheckpointTrigger: SavedProgressItemBase, ITriggerNotificationReceiver
    {
        [SerializeField, Self] private TriggerTrackerComponent trigger; 

        protected override void OnEnable()
        {
            base.OnEnable();
            trigger.RegisterReceiver(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            trigger.UnregisterReceiver(this);
        }

        public override void ApplySaveState(ProgressItemState state, ILevelProgressContext ctx)
        {
            switch (state)
            {
                case ProgressItemState.Default:
                    break;
                case ProgressItemState.Completed:
                    
                    trigger.UnregisterReceiver(this);
                    gameObject.SetActive(false);
                    break;
                case ProgressItemState.Failed:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public void TriggerEntered(TriggerHitInfo triggerHitInfo)
        {
            if (triggerHitInfo.TryGetEntityTarget(out var tgt) 
                && tgt.CompareTag("Player"))
            {
                Announce(ProgressItemState.Completed);
                LevelProgressController.Instance.OnCheckPointReached(this);
            }
        }

        public void TriggerExited(TriggerHitInfo triggerExitInfo)
        {
            //noop
        }
    }
}