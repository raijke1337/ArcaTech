using Arcatech.Managers;
using Arcatech.Texts;
using UnityEngine;

namespace Arcatech.Interactions
{
    /// <summary>
    /// display some text using the game interface manager
    /// </summary>
    public class DisplayTextInteractionHandler : InteractionHandlerBase
    {
        [SerializeField] DialoguePart text;
        //[SerializeField] bool requireInteraction = false;
        public override void DoInteraction(IInteractor interactor, IInteractive item = null)
        {
            GameInterfaceManager.Instance.HandleDialoguePart(text,true);
        }

        public override void EndInteraction(IInteractor interactor, IInteractive item = null)
        {
            GameInterfaceManager.Instance.HandleDialoguePart(text,false);
        }
    }
}