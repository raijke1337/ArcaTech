using Arcatech.Managers;
using Arcatech.Texts;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Interactions
{
    /// <summary>
    /// display some text using the game interface manager
    /// </summary>
    public class DisplayTextPassiveEvent : PassiveInteractionHandlerBase
    {

        [SerializeField] DialoguePart onEnter;
        [SerializeField] DialoguePart onExit;


        public override void OnInteractorEnter(IInteractor interactor)
        {
            GameInterfaceManager.Instance.ShowDialoguePart(onEnter, true);
            
        }

        public override void OnInteractorExit(IInteractor interactor)
        {
            GameInterfaceManager.Instance.ShowDialoguePart(onExit, true);
        }
    }
}