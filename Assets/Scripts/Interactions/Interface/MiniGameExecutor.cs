using Arcatech.Managers;
using Arcatech.MiniGames;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Interactions
{
    public class MiniGameExecutor : InteractionExecutor
    {
        [SerializeField] private MiniGameBase miniGamePrefab;

        private MiniGameBase _miniGame;
        private UnityAction<InteractionState> _completionCallback;

        public override bool CanCancel => true;

        private void OnDisable()
        {
            UnsubscribeFromGameResult();

            if (_miniGame != null)
            {
                _miniGame.EndGame();
            }
        }

        public override void Execute(
            InteractionContext ctx,
            UnityAction<InteractionState> onComplete)
        {
            EnsureMiniGameCreated();

            UnsubscribeFromGameResult();

            _completionCallback = onComplete;
            _miniGame.OnGameCompleteResult.AddListener(_completionCallback);

            _miniGame.StartGame();
        }

        public override void Cancel(InteractionContext ctx)
        {
            if (_miniGame != null)
            {
                _miniGame.EndGame();
            }

            UnsubscribeFromGameResult();

            base.Cancel(ctx);
        }

        private void EnsureMiniGameCreated()
        {
            if (_miniGame != null)
            {
                return;
            }

            if (miniGamePrefab == null)
            {
                Debug.LogError($"{nameof(MiniGameExecutor)}: Mini Game Prefab is not assigned.", this);
                return;
            }

            Transform parent = GameInterfaceManager.Instance.miniGame;

            _miniGame = Instantiate(miniGamePrefab, parent, false);
            _miniGame.gameObject.SetActive(false);
        }

        private void UnsubscribeFromGameResult()
        {
            if (_miniGame != null && _completionCallback != null)
            {
                _miniGame.OnGameCompleteResult.RemoveListener(_completionCallback);
            }

            _completionCallback = null;
        }
    }
}