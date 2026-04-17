using Arcatech.Managers;
using Arcatech.Texts;
using UnityEngine;

namespace Arcatech.Interactions
{
    public class DisplayTextInteraction : InteractionEventHandlerBase
    {
        [SerializeField] DialoguePart textSuccess;
        [SerializeField] DialoguePart textFailure;
        public override void DoInteraction(bool success, IInteractor interactor)
        {
            GameInterfaceManager.Instance.HandleDialoguePart(success ? textSuccess : textFailure, true);
        }
    }
}