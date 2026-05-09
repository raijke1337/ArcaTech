using System;
using Arcatech.Managers;
using Arcatech.MiniGames;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Interactions
{
    /// <summary>
    /// starts a minigame, minigame will write an interaction result into interaction context and advance the animation
    /// </summary>
    public class MiniGameExecutor : InteractionExecutor
    {
        [SerializeField] MiniGameBase miniGamePrefab;
        private  MiniGameBase _miniGame;
        public override bool CanCancel => true;

        public override void Execute(InteractionContext ctx, UnityAction<InteractionState> onComplete)
        {
            _miniGame = Instantiate(miniGamePrefab,GameInterfaceManager.Instance.miniGame);
            _miniGame.OnGameCompleteResult.AddListener(onComplete);
            _miniGame.StartGame();
        }

        public override void Cancel(InteractionContext ctx)
        {
            base.Cancel(ctx);
            _miniGame.EndGame();
        }
    }

}