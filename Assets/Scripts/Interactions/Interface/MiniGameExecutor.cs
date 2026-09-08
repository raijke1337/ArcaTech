using Arcatech.Managers;
using Arcatech.MiniGames;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

namespace Arcatech.Interactions
{
    public class MiniGameExecutor : InteractionExecutor
    {
        [SerializeField] private MiniGameBase miniGamePrefab;
        public override bool CanCancel => true;

        private UnityAction<InteractionState> _callback;

        public override void Execute(
            InteractionContext ctx,
            UnityAction<InteractionState> onComplete)
        {
            GameInterfaceManager.Instance.StartMinigame(miniGamePrefab,HandleGameResult);
            _callback = onComplete;
        }
        

        void HandleGameResult(InteractionState state)
        {
            _callback.Invoke(state);
            _callback = null;
        }
    }
}