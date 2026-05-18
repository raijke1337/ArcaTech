using Arcatech.SaveSystem;
using Arcatech.Units;
using DG.Tweening;
using UnityEngine;

namespace Arcatech.Interactions
{
    [RequireComponent(typeof(BaseGameEntityComponent))]
    public partial class ItemMovesEffect : InteractionEffect, IPausableComponent
    {
        
        // TODO: this whole part is now in "Item moves with Tween".
        // remove duplication
        
        [SerializeField] bool runFromEnable = true;
        [SerializeField] SerializedDOTweener tween;
        Tween cached;
        bool _pause = false;
        bool toggled = false;

        public bool Paused
        {
            get => _pause; 
            set
            {
                if (runFromEnable)
                {
                    cached.TogglePause();
                }
                else
                {
                    if (toggled) // activated by condition
                    {
                        cached.TogglePause();
                    }
                }
            }
        }
        private void OnEnable()
        {
            cached ??= tween.GetTween(transform).Pause();
            
            if (runFromEnable)
            {
                toggled = true;
                cached.Play(); 
            }
        }


        public override void Play(InteractionContext ctx)
        {
            if (ctx.State != InteractionState.Success) return;
            if (!runFromEnable) 
                cached.Play();
            toggled = true;
        }

        public override void OnLoadLevelState(ProgressItemState stateToLoad)
        {
            if (stateToLoad != ProgressItemState.Completed) return;
            toggled = true;
            cached ??= tween.GetTween(transform).Pause();
            cached.Play();
        }
    }

}
