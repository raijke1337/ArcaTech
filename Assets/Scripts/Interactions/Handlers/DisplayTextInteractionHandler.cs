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
        public override void DoInteraction(bool success, IInteractor interactor, IInteractive item)
        {
            // interactor.InteractionContext.ActiveGameUnitComponent.GetComponent<DashJumpMovementController>().SetDesiredMoveDirection(Vector3.zero);

            GameInterfaceManager.Instance.HandleDialoguePart(success? textSuccess : textFailure,true);
        }

        public override void EndInteraction(IInteractor interactor, IInteractive item = null)
        {
        }
    }
}