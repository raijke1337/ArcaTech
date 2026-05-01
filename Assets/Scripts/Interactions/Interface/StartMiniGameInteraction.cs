using Arcatech.Managers;
using Arcatech.MiniGames;
using UnityEngine;

namespace Arcatech.Interactions
{
    /// <summary>
    /// starts a minigame, minigame will write an interaction result into interaction context and advance the animation
    /// </summary>
    // public class StartMiniGameInteraction : InteractionEventHandlerBase
    // {
    //     [SerializeField] MiniGameBase miniGamePrefab;
    //     private  MiniGameBase _miniGame;
    //     private InteractionContext _ctx;
    //     
    //     public override void DoInteraction(bool success, IInteractor interactor)
    //     {
    //         if (success)
    //         {
    //             _ctx = interactor.InteractionContext;
    //             Debug.Log($"Starting game, interaction context info : has cached result {_ctx.HasInteractionResult(out var interactionResult)}, it is {interactionResult}");
    //             _miniGame = Instantiate(miniGamePrefab,GameInterfaceManager.Instance.transform);
    //             _miniGame.OnGameCompleteResult += ReportGameResult;
    //             _miniGame.StartGame();
    //         }
    //     }
    //
    //     private void ReportGameResult(bool result)
    //     {
    //         Debug.Log($"Finished game, writing result to context: {result}");
    //         _ctx.UpdateInteractionResult(result);
    //         _miniGame.OnGameCompleteResult -= ReportGameResult;
    //     }
    // }
}