using Arcatech.Managers;
using Arcatech.Texts;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Interactions
{
    /// <summary>
    /// display some text using the game interface manager
    /// </summary>
    public class DisplayTextInteractionHandler : InteractionHandlerBase
    {
        [SerializeField] DialoguePart textSuccess;
        [SerializeField] DialoguePart textFailure;
        [SerializeField] DialoguePart onEnter;
        [SerializeField] DialoguePart onExit;
        public override void DoInteraction(bool success, IInteractor interactor)
        {
            GameInterfaceManager.Instance.HandleDialoguePart(success? textSuccess : textFailure,true);
        }

        public override void OnPlayerEnter()
        {
            GameInterfaceManager.Instance.HandleDialoguePart(onEnter,true);
        }

        public override void OnPlayerExit()
        {
            GameInterfaceManager.Instance.HandleDialoguePart(onExit,true);
        }
    }
}