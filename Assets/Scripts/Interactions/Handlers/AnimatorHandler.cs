using System;
using UnityEngine;

namespace Arcatech.Interactions
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorHandler : InteractionHandlerBase
    {
        
        public string StartTrigger = "Start";
        public string EndTrigger = "End";
        public string UseTrigger = "Use";
        private int sHash;
        private int eHash;
        private int uHash;
        
        private Animator _animator;

        private void Start()
        {
            _animator = GetComponent<Animator>();
            sHash = Animator.StringToHash(StartTrigger);
            eHash = Animator.StringToHash(EndTrigger);
            uHash = Animator.StringToHash(UseTrigger);
        }

        public override void DoInteraction(bool success, IInteractor interactor)
        {
            if (success) _animator.SetTrigger(uHash);
        }
        

        public override void OnPlayerEnter()
        {
            _animator.SetTrigger(sHash);
        }

        public override void OnPlayerExit()
        {
            _animator.SetTrigger(eHash);
        }
    }
}