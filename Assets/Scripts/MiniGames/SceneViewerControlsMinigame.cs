using Arcatech.Interactions;
using Arcatech.Managers;
using UnityEngine.InputSystem;

namespace Arcatech.MiniGames
{
    public class SceneViewerControlsMinigame : MiniGameBase
    {
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

        public void UI_Retry()
        {
            ReportResult(InteractionState.Cancelled);
            GameInterfaceManager.Instance.OnRestartAtCheckpoint();
        }
    }
}