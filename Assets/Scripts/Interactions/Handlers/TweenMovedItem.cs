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
        [SerializeField]
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
        public override void DoInteraction(bool success, IInteractor interactor)
        {
            if (!success) return;
            if (!runFromEnable) 
                cached.Play();
            toggled = true;
        }
        
        public override void OnPlayerEnter()
        {
        }

        public override void OnPlayerExit()
        {
        }
    }

}
