using System;
using Arcatech.Managers;
using Arcatech.Texts;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Arcatech.Interactions
{
    public class ManualInteractionTrigger : InteractionTrigger, ITargetable
    {
        public override void TriggerEntered(TriggerHitInfo triggerHitInfo)
        {
            if (triggerHitInfo.TargetCollider.TryGetComponent(out IInteractor interactor))
            {
                interactor.RegisterInteractive(interactableComponent);
                if (interactor.Entity.ShowingDebugs)Debug.Log($"{interactor.Entity.GetName} has entered trigger area {name}");
            }
        }

        public override void TriggerExited(TriggerHitInfo triggerExitInfo)
        {
            if (triggerExitInfo.TargetCollider.TryGetComponent(out IInteractor interactor))
            {
                interactor.UnregisterInteractive(interactableComponent);
                if (interactor.Entity.ShowingDebugs) Debug.Log($"{interactor.Entity.GetName} has exited trigger are {name}");
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
          //  GameInterfaceManager.Instance?.NotifyTargetable(this,true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
           // GameInterfaceManager.Instance?.NotifyTargetable(this,false);
        }

        private void OnEnable()
        {
            interactableComponent.Entity.AssignName(GetInfo.Title);
        }

        // protected override void OnDisable()
        // {
        //     base.OnDisable();
        //     GameInterfaceManager.Instance?.NotifyTargetable(this,false);
        // }

        [Space,SerializeField] Description description;
        public Description GetInfo => description;
    }
}