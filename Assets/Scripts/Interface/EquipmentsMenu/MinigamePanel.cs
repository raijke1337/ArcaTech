using Arcatech.Interactions;
using Arcatech.MiniGames;
using KBCore.Refs;
using SpankyBoy.JuiceUI.Free;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.UI
{
    [RequireComponent(typeof(PanelAnimator))]
    public class MinigamePanel : ValidatedMonoBehaviour
    {             
        [SerializeField] protected Transform gamePanelParent;
        protected MiniGameBase instantiatedGame;
        protected UnityAction<InteractionState> cb;
        
        [SerializeField,Self] public PanelAnimator Animator;
        
        public virtual void LoadGame(MiniGameBase prefab, UnityAction<InteractionState> resultCallback)
        {
            if (instantiatedGame != null) Destroy(instantiatedGame.gameObject);

            if (prefab.ShowInGadget)
            {
                instantiatedGame = Instantiate(prefab, gamePanelParent);
            }
            else
            {
                instantiatedGame = Instantiate(prefab, transform.parent.transform);
            }

            cb = resultCallback;
            instantiatedGame.StartGame();

            instantiatedGame.onGameCompleteResult.AddListener(HandleGameResult);
        }
        void HandleGameResult(InteractionState state)
        {
            cb.Invoke(state);
            cb = null;
            Animator.Hide();
        }
    }
}