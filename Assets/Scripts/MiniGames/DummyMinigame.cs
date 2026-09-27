using Arcatech.Interactions;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Arcatech.MiniGames
{
    public class DummyMinigame : MiniGameBase
    {

        public void UI_ButtonSuccess()
        {
            ReportResult(InteractionState.Success);
        }
        public void UI_ButtonFail()
        {
            ReportResult(InteractionState.Failure);
        }

        public void UI_ButtonCancel()
        {
            ReportResult(InteractionState.Cancelled);
        }

        public override void ResetGame()
        {
            
        }

        protected override void OnGameStarted()
        {
        }

        protected override void OnGameEnded()
        {
        }

        protected override void HandleButtonPress(MiniGameButton button, InputAction.CallbackContext context)
        {
        }
    }
}