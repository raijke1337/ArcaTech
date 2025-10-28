using Arcatech.Units;
using DG.Tweening;
using NUnit.Framework;
using TMPro;
using UnityEngine;

namespace Arcatech.Interactions
{
    [RequireComponent(typeof(BaseGameEntityComponent))]
    public class TweenMovedItem : InteractionHandlerBase, IPausableComponent
    {
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
            cached = tween.GetTween(transform).Pause(); 
            if (runFromEnable)
            {
                toggled = true;
                cached.Play(); 
            }
        }
        public override void DoInteraction(IInteractor interactor, IInteractive item)
        {
            if (!runFromEnable) 
                cached.Play();
            toggled = true;
        }

        public override void EndInteraction(IInteractor interactor, IInteractive item = null)
        {
            // maybe TODO
        }
    }

}
