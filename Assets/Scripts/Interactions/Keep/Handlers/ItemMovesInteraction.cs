using Arcatech.Units;
using DG.Tweening;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Arcatech.Interactions
{
    [RequireComponent(typeof(BaseGameEntityComponent))]
    public partial class ItemMovesInteraction : InteractionEventHandlerBase, IPausableComponent
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
        public override void DoInteraction(bool success, IInteractor interactor)
        {
            if (!success) return;
            if (!runFromEnable) 
                cached.Play();
            toggled = true;
        }
        
    }

}
