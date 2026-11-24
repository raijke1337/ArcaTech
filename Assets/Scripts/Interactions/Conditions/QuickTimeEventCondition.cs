using Arcatech.Managers;
using Arcatech.MiniGames;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Interactions
{
    public sealed class QuickTimeEventCondition : InteractionCondition
    {
        [SerializeField] private MiniGameBase prefab;
        private UnityAction<InteractionState> _cb;
        [SerializeField] private bool invertResult = false; // used for cases where player failure starts the interaction (like enemy grabs)
        public override void Check(InteractionContext ctx, UnityAction<InteractionState> callback)
        {
            _cb = callback;
            GameInterfaceManager.Instance.StartQuickTime(prefab, HandleGameResult);
        }

        private void HandleGameResult(InteractionState result)
        {
            var cb = _cb;
            _cb = null;
            var newResult = result;
            if (invertResult)
            {
                newResult = result == InteractionState.Success?
                    InteractionState.Failure :
                    InteractionState.Success;
            }
            cb?.Invoke(newResult);
        }

        public override void CancelCheck(InteractionContext ctx)
        {
            // TODO: попросить GameInterfaceManager принудительно прервать текущий QTE,
            // если/когда там появится такой API.
            _cb = null;
        }
    }
}