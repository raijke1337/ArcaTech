using Arcatech.Interactions;
using UnityEngine.Events;

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
    }
}